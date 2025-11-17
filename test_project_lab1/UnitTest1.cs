using Microsoft.VisualStudio.TestTools.UnitTesting;
using AnalaizerClassLibrary;
using System;
using System.Text;
using MySql.Data.MySqlClient;

namespace test_project_lab1
{
    [TestClass]
    public class UnitTest1
    {
        // Рядок підключення до MySQL
        private string connectionString =
            "server=localhost;port=3306;database=calculator_tests;user=tester;password=1234;";

        /// <summary>
        /// Тести для Estimate, які зчитуються з бази даних
        /// Таблиця має мати стовпчики:
        /// Expression  (VARCHAR)
        /// Expected    (VARCHAR)
        /// </summary>
        [TestMethod]
        public void Estimate_FromDatabaseTestCases()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT Expression, Expected FROM estimate_tests";

                using (var command = new MySqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string input = reader.GetString("Expression");
                        string expected = reader.GetString("Expected");

                        AnalaizerClass.expression = input;
                        string actual = AnalaizerClass.Estimate();

                        Assert.AreEqual(
                            expected,
                            actual,
                            $"Помилка для виразу: {input}"
                        );
                    }
                }
            }
        }

        // -----------------------------------------------------------
        // Стандартні ручні тести
        // -----------------------------------------------------------

        [TestMethod]
        public void Estimate_SimpleAddition_ReturnsCorrectString()
        {
            AnalaizerClass.expression = "2+3";
            var result = AnalaizerClass.Estimate();
            Assert.AreEqual("5", result);
        }

        [TestMethod]
        public void Estimate_MixedPrecedence_ReturnsCorrectString()
        {
            AnalaizerClass.expression = "2+3*4";
            var result = AnalaizerClass.Estimate();
            Assert.AreEqual("14", result);
        }

        [TestMethod]
        public void Estimate_Parentheses_ReturnsCorrectString()
        {
            AnalaizerClass.expression = "(2+3)*4";
            var result = AnalaizerClass.Estimate();
            Assert.AreEqual("20", result);
        }

        [TestMethod]
        public void Estimate_UnaryMinusAtStart_ReturnsCorrectString()
        {
            AnalaizerClass.expression = "-5+2";
            var result = AnalaizerClass.Estimate();
            Assert.AreEqual("-3", result);
        }

        [TestMethod]
        public void Estimate_EmptyExpression_ReturnsEmptyString()
        {
            AnalaizerClass.expression = "";
            var result = AnalaizerClass.Estimate();
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void Estimate_InvalidSymbol_ReturnsErrorMarker()
        {
            AnalaizerClass.expression = "2+3a";
            var result = AnalaizerClass.Estimate();
            Assert.IsTrue(result.StartsWith("&"));
        }

        [TestMethod]
        public void Estimate_DivideByZero_ReturnsErrorMarker()
        {
            AnalaizerClass.expression = "10/0";
            var result = AnalaizerClass.Estimate();
            Assert.IsTrue(result.StartsWith("&"));
        }

        [TestMethod]
        public void Estimate_UnmatchedOpeningBracket_ReturnsErrorMarker()
        {
            AnalaizerClass.expression = "(2+3";
            var result = AnalaizerClass.Estimate();
            Assert.IsTrue(result.StartsWith("&"));
        }

        [TestMethod]
        public void Estimate_TooDeepBrackets_ReturnsErrorMarker()
        {
            AnalaizerClass.expression = "((((1))))";
            var result = AnalaizerClass.Estimate();
            Assert.IsTrue(result.StartsWith("&"));
        }

        [TestMethod]
        public void Estimate_TwoOperatorsInRow_ReturnsCorrectUnaryPlusResult()
        {
            AnalaizerClass.expression = "2++3";
            var result = AnalaizerClass.Estimate();
            Assert.AreEqual("5", result);
        }

        [TestMethod]
        public void Estimate_EndsWithOperator_ReturnsErrorMarker()
        {
            AnalaizerClass.expression = "2+";
            var result = AnalaizerClass.Estimate();
            Assert.IsTrue(result.StartsWith("&"));
        }

        [TestMethod]
        public void Estimate_ExcessiveLengthExpression_ReturnsErrorMarker()
        {
            var longExpr = new string('1', 65537);
            AnalaizerClass.expression = longExpr;
            var result = AnalaizerClass.Estimate();
            Assert.IsTrue(result.StartsWith("&"));
        }
    }
}
