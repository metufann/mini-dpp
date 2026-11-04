using QRCoder;

namespace MiniDpp.Api.Services;

public class QrCodeService(PublicUrlBuilder publicUrlBuilder)
{
    public byte[] CreatePngForProduct(string gtin)
    {
        var targetUrl = publicUrlBuilder.HtmlDpp(gtin);
        using var generator = new QRCodeGenerator();
        using var qrCodeData = generator.CreateQrCode(targetUrl, QRCodeGenerator.ECCLevel.Q);
        var pngQrCode = new PngByteQRCode(qrCodeData);
        return pngQrCode.GetGraphic(8);
    }
}
