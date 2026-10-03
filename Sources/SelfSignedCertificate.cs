using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Zhixiang.WindowsHttpsSample;

public static class SelfSignedCertificate
{
    public static X509Certificate2 EnsureCertificate()
    {
        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadWrite);

        var existingCertificate = store.Certificates
            .Find(X509FindType.FindBySubjectDistinguishedName, "CN=localhost", validOnly: false)
            .Cast<X509Certificate2>()
            .FirstOrDefault(candidate => candidate.HasPrivateKey && candidate.Subject.Contains("localhost", StringComparison.OrdinalIgnoreCase));

        if (existingCertificate is not null)
        {
            return existingCertificate;
        }

        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=localhost", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, true));
        request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension([new Oid("1.3.6.1.5.5.7.3.1")], true));
        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

        var subjectAlternativeNames = new SubjectAlternativeNameBuilder();
        subjectAlternativeNames.AddDnsName("localhost");
        subjectAlternativeNames.AddIpAddress(IPAddress.Loopback);
        subjectAlternativeNames.AddIpAddress(IPAddress.IPv6Loopback);
        request.CertificateExtensions.Add(subjectAlternativeNames.Build());

        var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));
        var certificateBytes = certificate.Export(X509ContentType.Pfx);
        var exportableCertificate = X509CertificateLoader.LoadPkcs12(certificateBytes, (string?)null, X509KeyStorageFlags.Exportable);

        store.Add(exportableCertificate);
        return exportableCertificate;
    }
}
