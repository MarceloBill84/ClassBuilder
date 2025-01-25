using ClassBuilder.Constanst;
using ClassBuilder.Exceptions;
using ClassBuilder.Factories;

namespace ClassBuilder.Tests.Factories
{
    public class BuilderFactoryTests
    {
        [Fact]
        public void TestDefaultClass()
        {
            var fileContent = @"using System;

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

        [Fact]
        public void TestStruct()
        {
            var fileContent = @"public struct Pessoa
{
    public int Idade { get; set; }
    public string Nome { get; set; }
    public string Cpf { get; set; }
    public string Email { get; set; }

    public override string ToString()
    {
        return $""Nome: {this.Nome}"" +
               $""Email: {this.Email}"";
    }
}";

            var result = BuilderFactory.Create(fileContent);

            Assert.NotNull(result);
            Assert.Contains($"{Constants.PrefixMethodName}Idade", result);
            Assert.Contains($"{Constants.PrefixMethodName}Nome", result);
            Assert.Contains($"{Constants.PrefixMethodName}Cpf", result);
            Assert.Contains($"{Constants.PrefixMethodName}Email", result);
            Assert.DoesNotContain("ToString", result);
        }

        [Fact]
        public void TestMultiClassInFile()
        {
            var fileContent = @"using System;

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



    public struct Pessoa
    {
        public int Idade { get; set; }
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public string Email { get; set; }

        public override string ToString()
        {
            return $""Nome: {this.Nome}"" +
                   $""Email: {this.Email}"";
        }
    }
}";

            var result = BuilderFactory.Create(fileContent);

            Assert.NotNull(result);
            Assert.Contains($"LoginViewModel{Constants.SuffixClassName}", result);
            Assert.Contains($"{Constants.PrefixMethodName}Name", result);
            Assert.Contains($"{Constants.PrefixMethodName}Password", result);
            Assert.Contains($"{Constants.PrefixMethodName}Longitude", result);
            Assert.Contains($"{Constants.PrefixMethodName}Latitude", result);
            Assert.DoesNotContain("Total", result);

            Assert.Contains($"Pessoa{Constants.SuffixClassName}", result);
            Assert.Contains($"{Constants.PrefixMethodName}Idade", result);
            Assert.Contains($"{Constants.PrefixMethodName}Nome", result);
            Assert.Contains($"{Constants.PrefixMethodName}Cpf", result);
            Assert.Contains($"{Constants.PrefixMethodName}Email", result);
            Assert.DoesNotContain("ToString", result);
        }

        [Fact]
        public void TestEmptyClass()
        {
            var fileContent = @"using System;

namespace Authentication.Application.ViewModels
{
	public class LoginViewModel
	{
		
	}
}";

            Assert.Throws<ValidationException>(() => BuilderFactory.Create(fileContent));
        }

        [Fact]
        public void TestFileWithoutClass()
        {
            var fileContent = @"using System;

namespace Authentication.Application.ViewModels
{	
}";

            Assert.Throws<ValidationException>(() => BuilderFactory.Create(fileContent));
        }

        [Fact]
        public void TestFileWithoutUsings()
        {
            var fileContent = @"namespace ClassBuilder.Dto
{
    public class PropertyInfo
    {
        public string Type { get; }
        public string Name { get; }

        public PropertyInfo(string type, string name)
        {
            Type = type;
            Name = name;
        }
    }
}
";

            var result = BuilderFactory.Create(fileContent);

            Assert.NotNull(result);
            Assert.Contains($"PropertyInfo{Constants.SuffixClassName}", result);
            Assert.Contains($"{Constants.PrefixMethodName}Name", result);
            Assert.Contains($"{Constants.PrefixMethodName}Type", result);
        }
    }
}