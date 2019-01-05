using System;
using Xunit;
using Moq;
using Moq.Protected;

namespace CreditCardApplication.Tests
{
    public class CreditCardApplicationEvaluatorShould
    {
        private Mock<IFrequentFlayerNumberValidator>  mockValidator;
        private CreditCardApplicationEvaluator sut;

        public CreditCardApplicationEvaluatorShould()
        {
            mockValidator = new Mock<IFrequentFlayerNumberValidator>();
            mockValidator.SetupAllProperties();
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);

            sut = new CreditCardApplicationEvaluator(mockValidator.Object);
        }

        [Fact]
        public void AcceptHighIncomeApplications()
        {
            var application = new CreditCardApplication{GrossAnnualIncome = 100_000};
            var decision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.AutoAccepted, decision);
        }

        [Fact]
        public void ReferYoungApplications()
        {
            var application = new CreditCardApplication { Age = 19 };
            var decision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void DeclineLowIncomeApplications()
        {
            // mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            // mockValidator.Setup(x => x.IsValid(It.Is<string>( number => number.StartsWith("x")))).Returns(true);
            // mockValidator.Setup(x => x.IsValid(It.IsIn("x", "y", "z"))).Returns(true);
            // mockValidator.Setup(x => x.IsValid(It.IsInRange("b", "z", Range.Inclusive))).Returns(true);
            mockValidator.Setup(x => x.IsValid(It.IsRegex("[a-z]", System.Text.RegularExpressions.RegexOptions.None))).Returns(true);
            mockValidator.DefaultValue = DefaultValue.Mock;

            var application = new CreditCardApplication {
                GrossAnnualIncome = 19_000,
                Age = 42,
                FrequentPlayerNumber = "a"
            };

            var decision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }

        [Fact]
        public void ReferInvalidFrequentFlayerApplications()
        {
            mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(false);
            mockValidator.DefaultValue = DefaultValue.Mock;

            sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            var application = new CreditCardApplication();

            var decision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        //[Fact]
        //public void DeclineLowIncomeApplicationsOutDemo()
        //{
        //    var mockValidator = new Mock<IFrequentFlayerNumberValidator>();

        //    var isValid = true;
        //    mockValidator.Setup(x => x.IsValid(It.IsAny<string>(), out isValid));
           
        //    var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

        //    var application = new CreditCardApplication
        //    {
        //        GrossAnnualIncome = 19_000,
        //        Age = 42,
        //        FrequentPlayerNumber = "x"
        //    };

        //    var decision = sut.EvaluateUsingOut(application);
        //    Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        //}

        [Fact]
        public void ReferWhenLicenseKeyExpired()
        {
           
            //mockValidator.Setup(x => x.LicenseKey).Returns(GetLicenseKeyExpiredString);
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("EXPIRED");
            
            sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication { Age = 42 };

            var decision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void UseDetailedLookUpForOlderApplications()
        {
            // mockValidator.SetupProperty(x => x.ValidationMode); // Track changes for ValidationMode property
            var application = new CreditCardApplication { Age = 30};

            var decision = sut.Evaluate(application);
            Assert.Equal(ValidationMode.Detailed, mockValidator.Object.ValidationMode);
        }

        [Fact]
        public void ValidateFrequentFlayerNumberForLowIncomeApplications()
        {
            var application = new CreditCardApplication{ FrequentPlayerNumber = "q"};
            sut.Evaluate(application);

            //Verify that the method IsValid was execute with "q" as parameters
            //mockValidator.Verify(x => x.IsValid("q"));
            mockValidator.Verify(x => x.IsValid(It.IsAny<string>()), Times.Once); // Times.Once we are exepcting to be executed only one time.
        }

        [Fact]
        public void ShouldValidateFrequentFlayerNumberForLowIncomeApplications_CustomMessage()
        {
            var application = new CreditCardApplication { FrequentPlayerNumber = "q" };
            //var application = new CreditCardApplication(); uncomment this to see the test fail and the error message
            sut.Evaluate(application);

            //Verify that the method IsValid was execute with "q" as parameters
            //mockValidator.Verify(x => x.IsValid("q"));
            mockValidator.Verify(x => x.IsValid(It.IsNotNull<string>()), "Frequent flayer number should not be null");
        }

        [Fact]
        public void NotValidFrequentFlayerNumberForHighIncomeApplications()
        {
            var application = new CreditCardApplication { GrossAnnualIncome = 100_000};
            
            sut.Evaluate(application);

            // Having big income we are not expecting for the IsValid() method to be executed.
            // and the last variable Times.Never means that the method should not be executed
            mockValidator.Verify(x => x.IsValid(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void CheckLicenseKeyForLowIncomeApplications()
        {
            var application = new CreditCardApplication { GrossAnnualIncome = 99_000 };

            sut.Evaluate(application);


            //We are veryfing that the "property GET" executed once. We don't care about the property value.
            mockValidator.VerifyGet(x => x.ServiceInformation.License.LicenseKey, Times.Once);
        }

        [Fact]
        public void SetDetailedLookUpForOlderApplications()
        {
            var application = new CreditCardApplication { Age = 30};

            sut.Evaluate(application);

            //We are veryfing that the "property SET" executed once with any value
            mockValidator.VerifySet(x => x.ValidationMode = It.IsAny<ValidationMode>(), Times.Once);
        }

        [Fact]
        public void ReferWhenFrequentFlayerValidationError()
        {
            mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Throws(new Exception("Custom message"));

            sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication { Age = 42};

            var decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void IncrementLookupCpunt()
        {
            mockValidator.Setup(x => x.IsValid(It.IsAny<string>()))
                .Returns(true)
                .Raises(x => x.ValidatorLookupPerformed += null, EventArgs.Empty);

            sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication { FrequentPlayerNumber = "x", Age = 42 };

            sut.Evaluate(application);

            //Raise an event
            //mockValidator.Raise(x => x.ValidatorLookupPerformed += null, EventArgs.Empty);

            Assert.Equal(1, sut.ValidatorLookupCount);
        }

        [Fact]
        public void ReferInvalidFrequentFlayerApplications_Sequence()
        {
            mockValidator.SetupSequence(x => x.IsValid(It.IsAny<string>()))
                .Returns(false)
                .Returns(true);

            sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication { Age = 25 };

            var firstDecision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, firstDecision);

            var secondDecision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, secondDecision);
        }

        [Fact]
        public void ReferFraudRisk()
        {
            var mockFraudLookup = new Mock<FraudLookup>();
            
            //mockFraudLookup.Setup(x => x.IsFraudRisk(It.IsAny<CreditCardApplication>())).Returns(true);
            mockFraudLookup.Protected()
                .Setup<bool>("CheckApplication", ItExpr.IsAny<CreditCardApplication>())
                .Returns(true);

            sut = new CreditCardApplicationEvaluator(mockValidator.Object, mockFraudLookup.Object);

            var application = new CreditCardApplication();

            var decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHumanFraudRisk, decision);
        }

        [Fact]
        public void LinqToMocks()
        {
            //var mockValidator = new Mock<IFrequentFlayerNumberValidator>();
            //mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            //mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);

            IFrequentFlayerNumberValidator mockValidator = Mock.Of<IFrequentFlayerNumberValidator>(
                   validator => validator.ServiceInformation.License.LicenseKey == "OK" &&
                                validator.IsValid(It.IsAny<string>()) == true
            );

            var sut = new CreditCardApplicationEvaluator(mockValidator);

            var application = new CreditCardApplication { Age = 25 };

            var decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }

        private string GetLicenseKeyExpiredString()
        {
            return "EXPIRED";
        }
    }
}
