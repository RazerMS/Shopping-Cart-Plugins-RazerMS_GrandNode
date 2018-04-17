using Grand.Core.Domain.Payments;

namespace Grand.Plugin.Payments.MOLPay
{
    public class MOLPayHelper
    {
        public static string OrderTotalSentToMOLPay => "OrderTotalSentToMOLPay";

        /// <summary>
        /// Gets a payment status
        /// </summary>
        /// <param name="paymentStatus">PayPal payment status</param>
        /// <param name="pendingReason">PayPal pending reason</param>
        /// <returns>Payment status</returns>
        public static PaymentStatus GetPaymentStatus(string paymentStatus)
        {
            var result = PaymentStatus.Pending;

            if (paymentStatus == null)
                paymentStatus = string.Empty;

            switch (paymentStatus)
            {
                case "22":
                    result = PaymentStatus.Pending;
                    break;
                case "00":
                    result = PaymentStatus.Paid;
                    break;
                case "11":
                    result = PaymentStatus.Voided;
                    break;
                default:
                    break;
            }
            return result;
        }
    }
}
