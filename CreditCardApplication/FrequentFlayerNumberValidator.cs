using System;

namespace CreditCardApplication
{
    public class FrequentFlayerNumberValidator: IFrequentFlayerNumberValidator
    {
        public bool IsValid(string frequentFlayerNumber)
        {
            throw new NotImplementedException();
        }

        public void IsValid(string frequentFlayerNumber, out bool isvalid)
        {
            throw new NotImplementedException();
        }

        public IServiceInformation ServiceInformation
        {
            get => throw new NotImplementedException("For demo purposes");
        }

        public ValidationMode ValidationMode
        {
            get => throw new NotImplementedException("For demo purposes");
            set => throw new NotImplementedException("For demo purposes");
        }

        public event EventHandler ValidatorLookupPerformed;
    }
}
