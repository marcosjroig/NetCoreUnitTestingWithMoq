using System;

namespace CreditCardApplication
{
    public interface ILicenseData
    {
        string LicenseKey { get; }
    }

    public interface IServiceInformation
    {
        ILicenseData License { get; set; }
    }

    public enum ValidationMode
    {
        Quick,
        Detailed
    }

    public interface IFrequentFlayerNumberValidator
    {
        bool IsValid(string frequentFlayerNumber);
        void IsValid(string frequentFlayerNumber, out bool isvalid);
        IServiceInformation ServiceInformation { get; }
        ValidationMode ValidationMode { get; set; }
        event EventHandler ValidatorLookupPerformed;
    }
}
