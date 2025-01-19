using ClassBuilder.Constanst;
using ClassBuilder.Factories;

namespace ClassBuilder.Tests.Factories
{
    public class BuilderFactoryTests
    {
        [Fact]
        public void TestDefaultClass()
        {
            var fileContent = @"using System;
using lerolero;

namespace Authentication.Application.ViewModels
{
	public class LoginViewModel
	{
		public string Name { get; set; }
		public string Password { get; set; }
		public decimal? Longitude { get; set; }
		public decimal Latitude { get; set; }

        public decimal Total()
        {
            return Longitude + Latitude;
        }
	}
}";

            var result = BuilderFactory.Create(fileContent);

            Assert.NotNull(result);
            Assert.Contains($"{Constants.PrefixMethodName}Name", result);
            Assert.Contains($"{Constants.PrefixMethodName}Password", result);
            Assert.Contains($"{Constants.PrefixMethodName}Longitude", result);
            Assert.Contains($"{Constants.PrefixMethodName}Latitude", result);
            Assert.DoesNotContain("Total", result);
        }

        [Fact]
        public void TestPrimaryConstructor()
        {
            var fileContent = @"
namespace Authentication.Application.ViewModels;

public class BankAccount(string accountID, string owner)
{
    public string AccountID { get; } = accountID;
    public string Owner { get; } = owner;

    public override string ToString() => $""Account ID: {AccountID}, Owner: {Owner}"";
}";

            var result = BuilderFactory.Create(fileContent);

            Assert.NotNull(result);
            Assert.Contains($"{Constants.PrefixMethodName}AccountID", result);
            Assert.Contains($"{Constants.PrefixMethodName}Owner", result);
            Assert.DoesNotContain("ToString", result);
        }

        [Fact]
        public void TestRecord()
        {
            var fileContent = @"namespace ClassBuilder.Dto
{
    public record Cliente
    {
        public string Nome { get; init; }
        public string Email { get; init; }
    }
}
";

            var result = BuilderFactory.Create(fileContent);

            Assert.NotNull(result);
            Assert.Contains($"{Constants.PrefixMethodName}Nome", result);
            Assert.Contains($"{Constants.PrefixMethodName}Email", result);
        }

        [Fact]
        public void TestDataMember()
        {
            var fileContent = @"public class MyClass
{
    private string fullName;

    [DataMember]
   public string FullName
   {
       get { return fullName; }
       set { fullName = value; }
   }
}
";

            var result = BuilderFactory.Create(fileContent);

            Assert.NotNull(result);
            Assert.Contains($"{Constants.PrefixMethodName}FullName", result);
        }
    }
}