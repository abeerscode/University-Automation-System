using UA.Facade.DTO.Finance;
namespace UA.Facade.Interface.Integration;
public interface IReceiptPdfWriter { byte[] Write(ReceiptDto receipt); }
