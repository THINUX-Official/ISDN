using ISDN.Models;

namespace ISDN.Interfaces
{
    public interface IPaymentRepository
    {
        Customer GetFirstCustomer(); // පළමු පාරිභෝගිකයාගේ දත්ත ගැනීමට
        void SavePayment(Payment payment); // ගෙවීම් දත්ත Save කිරීමට
    }
}