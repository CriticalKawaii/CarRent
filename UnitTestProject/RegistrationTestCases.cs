using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfApp;

namespace UnitTestProject
{
    [TestClass][TestCategory("Регистрация")]
    public class RegistrationTestCases : WpfTestBase
    {
        private SignUpPage signUpPage;
        [TestInitialize]
        public void Setup() => signUpPage = new SignUpPage();

        [DataTestMethod]
        [DataRow("test_unique1@email.com", "John", "Doe", "SecurePass1", "SecurePass1", true, DisplayName = "Корректные данные регистрации")]
        [DataRow("", "John", "Doe", "SecurePass1", "SecurePass1", false, DisplayName = "Пустой email")]
        [DataRow("not-an-email", "John", "Doe", "SecurePass1", "SecurePass1", false, DisplayName = "Некорректный формат email")]
        [DataRow("test_unique2@email.com", "John", "Doe", "Pass1", "Pass1", false, DisplayName = "Пароль слишком короткий")]
        [DataRow("test_unique3@email.com", "John", "Doe", "SecurePass1", "DifferentPass", false, DisplayName = "Пароли не совпадают")]
        [DataRow("user@email.com", "John", "Doe", "SecurePass1", "SecurePass1", false, DisplayName = "Email уже существует")]
        [DataRow("test_unique1@email.com", "John1", "Doe", "SecurePass1", "SecurePass1", false, DisplayName = "Неорректное имя")]
        public void Register_WithVariousInputs_ReturnsExpectedResults(string email, string firstName, string lastName,
            string password, string confirmPassword, bool expectedResult)
        =>Assert.AreEqual(expectedResult, signUpPage.Register(email, firstName, lastName, password, confirmPassword, skip: true),
                $"Регистрация с email: {email}, именем: {firstName} {lastName}, " +
                $"сравнением паролей: {password == confirmPassword} должна вернуть {expectedResult}");
    }
}
