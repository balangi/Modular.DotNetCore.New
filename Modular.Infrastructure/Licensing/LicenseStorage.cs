using System.Security.Cryptography;
using System.Text.Json;

namespace Modular.Infrastructure.Runtime.Licensing;

public interface ILicenseStorage
{
    ValueTask<string?> ReadAsync(CancellationToken cancellationToken = default);
    ValueTask WriteAsync(string licenseToken, CancellationToken cancellationToken = default);
    ValueTask DeleteAsync(CancellationToken cancellationToken = default);
}

/// <summary>AES-GCM encrypted, atomically replaced file store. Key must be kept outside the license file.</summary>
public sealed class EncryptedFileLicenseStorage : ILicenseStorage
{
    private const int NonceSize = 12, TagSize = 16;
    private readonly string _path;
    private readonly byte[] _key;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public EncryptedFileLicenseStorage(string path, byte[] aesKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (aesKey is null || aesKey.Length is not (16 or 24 or 32))
            throw new ArgumentException("AES key must contain 16, 24, or 32 bytes.", nameof(aesKey));
        _path = Path.GetFullPath(path);
        _key = aesKey.ToArray();
    }

    public async ValueTask<string?> ReadAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!File.Exists(_path)) return null;
            var envelope = JsonSerializer.Deserialize<Envelope>(await File.ReadAllBytesAsync(_path, cancellationToken).ConfigureAwait(false), _json)
                ?? throw new InvalidDataException("License storage envelope is empty.");
            var nonce = Convert.FromBase64String(envelope.Nonce);
            var cipher = Convert.FromBase64String(envelope.Ciphertext);
            var tag = Convert.FromBase64String(envelope.Tag);
            if (nonce.Length != NonceSize || tag.Length != TagSize) throw new InvalidDataException("Invalid encrypted license envelope.");
            var plaintext = new byte[cipher.Length];
            using var aes = new AesGcm(_key, TagSize);
            aes.Decrypt(nonce, cipher, tag, plaintext);
            return System.Text.Encoding.UTF8.GetString(plaintext);
        }
        finally { _gate.Release(); }
    }

    public async ValueTask WriteAsync(string licenseToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(licenseToken);
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var dir = Path.GetDirectoryName(_path)!;
            Directory.CreateDirectory(dir);
            var nonce = RandomNumberGenerator.GetBytes(NonceSize);
            var plaintext = System.Text.Encoding.UTF8.GetBytes(licenseToken);
            var cipher = new byte[plaintext.Length]; var tag = new byte[TagSize];
            using (var aes = new AesGcm(_key, TagSize)) aes.Encrypt(nonce, plaintext, cipher, tag);
            var envelope = JsonSerializer.SerializeToUtf8Bytes(new Envelope(Convert.ToBase64String(nonce), Convert.ToBase64String(cipher), Convert.ToBase64String(tag)), _json);
            var temp = _path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try { await File.WriteAllBytesAsync(temp, envelope, cancellationToken).ConfigureAwait(false); File.Move(temp, _path, true); }
            finally { if (File.Exists(temp)) File.Delete(temp); }
            CryptographicOperations.ZeroMemory(plaintext);
        }
        finally { _gate.Release(); }
    }

    public async ValueTask DeleteAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try { cancellationToken.ThrowIfCancellationRequested(); if (File.Exists(_path)) File.Delete(_path); }
        finally { _gate.Release(); }
    }

    private sealed record Envelope(string Nonce, string Ciphertext, string Tag);
}
