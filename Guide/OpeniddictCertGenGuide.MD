# OpenIddict Certificate Generation Guide

## Option 1: Self-Signed Certificate for Development/Internal Use

### Windows (PowerShell)
```powershell
# Generate certificate
$cert = New-SelfSignedCertificate `
    -Subject "CN=IbraHabra.IAM" `
    -KeyAlgorithm RSA `
    -KeyLength 2048 `
    -NotAfter (Get-Date).AddYears(5) `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -KeyExportPolicy Exportable `
    -KeyUsage DigitalSignature, KeyEncipherment `
    -Type DocumentEncryptionCert

# Export with password
$password = ConvertTo-SecureString -String "YourStrongPassword123!" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath "iam-certificate.pfx" -Password $password
```

### Linux/macOS (OpenSSL)
```bash
# Generate private key
openssl genrsa -out iam-private.key 2048

# Create certificate signing request
openssl req -new -key iam-private.key -out iam.csr \
  -subj "/C=US/ST=State/L=City/O=IbraHabra/CN=IbraHabra.IAM"

# Generate self-signed certificate (valid 5 years)
openssl x509 -req -days 1825 -in iam.csr \
  -signkey iam-private.key -out iam-certificate.crt

# Create PFX file with password
openssl pkcs12 -export -out iam-certificate.pfx \
  -inkey iam-private.key -in iam-certificate.crt \
  -password pass:YourStrongPassword123!
```

## Option 2: Production Certificate (Recommended)

For production, use a certificate from a trusted CA:
- Let's Encrypt (free, automated)
- DigiCert, Sectigo, etc. (paid)
- Your organization's internal CA

## Configuration

### appsettings.Production.json
```json
{
  "OpenIddict": {
    "Certificate": {
      "Path": "/app/certs/iam-certificate.pfx",
      "Password": null  // Will use CERT_PASSWORD env var
    }
  }
}
```

### Environment Variables
```bash
# Docker
docker run -e CERT_PASSWORD="YourStrongPassword123!" \
  -v /path/to/certs:/app/certs \
  your-iam-image

# Kubernetes Secret
kubectl create secret generic iam-cert \
  --from-file=certificate=/path/to/iam-certificate.pfx \
  --from-literal=password=YourStrongPassword123!
```

### appsettings.Development.json
```json
{
  "OpenIddict": {
    "Certificate": {
      "Path": null  // Uses AddDevelopmentEncryptionCertificate()
    }
  }
}
```

## Security Best Practices

### ✅ DO
- Store certificates outside source control
- Use environment variables for passwords
- Rotate certificates before expiry
- Use 2048-bit RSA minimum (4096 for high security)
- Keep private keys secure with proper file permissions (`chmod 600`)
- Use different certificates for signing and encryption (optional but recommended)
- Monitor certificate expiry dates

### ❌ DON'T
- Commit certificates to Git
- Hardcode passwords in configuration
- Use the same certificate across environments
- Share certificates between services
- Use certificates with weak algorithms (MD5, SHA1)

## Certificate Rotation

```csharp
// In production, implement certificate rotation
public static void ConfigureMultipleCertificates(this OpenIddictServerBuilder opts, IConfiguration config)
{
    // Primary (current) certificate - Use X509CertificateLoader (.NET 8+)
    var primaryCert = X509CertificateLoader.LoadPkcs12FromFile(
        "primary-cert.pfx", 
        "password",
        X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);
    opts.AddSigningCertificate(primaryCert);
    
    // Secondary (next) certificate - clients can validate both during rotation
    var secondaryCertPath = config["OpenIddict:Certificate:SecondaryPath"];
    var secondaryPassword = config["OpenIddict:Certificate:SecondaryPassword"];
    
    if (!string.IsNullOrEmpty(secondaryCertPath) && File.Exists(secondaryCertPath))
    {
        var secondaryCert = X509CertificateLoader.LoadPkcs12FromFile(
            secondaryCertPath, 
            secondaryPassword,
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);
        opts.AddSigningCertificate(secondaryCert);
    }
}
```

## Verification

After setup, verify your configuration:

```bash
# Check JWKS endpoint (public keys)
curl https://your-iam.com/.well-known/openid-configuration

# Should show discovery document with jwks_uri
curl https://your-iam.com/.well-known/jwks.json

# Should show public keys in JWK format
```

## Summary

| Use Case | Key Type | Configuration |
|----------|----------|---------------|
| **Admin JWT** | Symmetric (HMAC-SHA256) | Keep current symmetric key |
| **OpenIddict (Dev)** | Asymmetric (RSA) | `AddDevelopmentCertificate()` |
| **OpenIddict (Prod)** | Asymmetric (RSA) | PFX certificate file |

The asymmetric keys in OpenIddict allow external clients to verify tokens using your public key from the JWKS endpoint, while your private key remains secure.