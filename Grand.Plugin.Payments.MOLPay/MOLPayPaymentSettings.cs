using Grand.Core.Configuration;
using Grand.Core.Domain.Payments;

namespace Grand.Plugin.Payments.MOLPay
{
    public class MOLPayPaymentSettings : ISettings
    {
        public bool UseSandbox { get; set; }
        public string MerchantId { get; set; }
        public string Vkey { get; set; }
        public string Skey { get; set; }

        public string ChannelType { get; set; }
        public string PdtToken { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }
        /// <summary>
        /// Additional fee
        /// </summary>
        public decimal AdditionalFee { get; set; }

        public PaymentStatus CapturedMode { get; set; }

        public PaymentStatus PendingMode { get; set; }

        public PaymentStatus FailedMode { get; set; }


        public bool PassProductNamesAndTotals { get; set; }
        public bool PdtValidateOrderTotal { get; set; }
    }
}
