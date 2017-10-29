using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
    [TestFixture]
	public class NumberValidatorTests
    {
        public class Constructor_Should
        {
            [TestCase(0, 2, true)]
            [TestCase(-1, 1, false)]
            public void ThrowArgumentException_On_NonPositivePrecision(int precision, int scale, bool onlyPositive)
            {
                Assert.Throws<ArgumentException>(() => new NumberValidator(precision, scale, onlyPositive));
            }

            [TestCase(1, -1, true)]
            [TestCase(2, -2, false)]
            public void ThrowArgumentException_On_NegativeScale(int precision, int scale, bool onlyPositive)
            {
                Assert.Throws<ArgumentException>(() => new NumberValidator(precision, scale, onlyPositive));
            }

            [TestCase(3, 3, false)]
            [TestCase(3, 5, true)]
            public void ThrowArgumentException_On_ScaleMoreOrEqualToPrecision(int precision, int scale, bool onlyPositive)
            {
                Assert.Throws<ArgumentException>(() => new NumberValidator(precision, scale, onlyPositive));
            }
            
            [TestCase(5, 4, false)]
            [TestCase(1, 0, true)]
            public void NotThrowException_On_ValidArguments(int precision, int scale, bool onlyPositive)
            {
                // кажется, этот тест не нужен.
                Assert.DoesNotThrow(() => new NumberValidator(precision, scale, onlyPositive));
            }
        }


        public class IsValidNumber_ShouldBe
        {
            [TestCase(2, 1, true, null)]
            [TestCase(4, 3, false, "")]
            public void False_OnNullOrEmpty(int precision, int scale, bool onlyPositive, string value)
            {
                new NumberValidator(precision, scale, onlyPositive).IsValidNumber(value).Should().BeFalse();
            }
            
            [TestCase(6, 4, false, "+-0.0")]
            [TestCase(6, 4, false, "--0.0")]
            [TestCase(6, 4, false, "-+-0")]
            public void False_OnMultiSign(int precision, int scale, bool onlyPositive, string value)
            {
                new NumberValidator(precision, scale, onlyPositive).IsValidNumber(value).Should().BeFalse();
            }
            
            [TestCase(6, 4, "-0")]
            [TestCase(6, 4, "-0.0")]
            [TestCase(6, 4, "-3.1415")]
            [TestCase(6, 5, "-0.00000000000001")]
            public void False_OnNegatives_When_OnlyPositive(int precision, int scale, string value)
            {
                new NumberValidator(precision, scale, onlyPositive:true).IsValidNumber(value).Should().BeFalse();
            }
            
            [TestCase(3, 2, false, "10.10")]
            [TestCase(3, 2, true, "10.10")]
            [TestCase(3, 2, false, "010.10")]
            [TestCase(4, 3, false, "-1.001")]
            public void False_OnPrecisionViolation(int precision, int scale, bool onlyPositive, string value)
            {
                new NumberValidator(precision, scale, onlyPositive).IsValidNumber(value).Should().BeFalse();
            }
            
            [TestCase(3, 2, false, "0.100")]
            [TestCase(20, 5, false, "20.18181818")]
            [TestCase(20, 5, true, "20.18181818")]
            [TestCase(20, 5, false, "-20.181810000")]
            [TestCase(20, 5, true, "20.181810000")]
            [TestCase(20, 0, true, "20.")]
            public void False_OnScaleViolation(int precision, int scale, bool onlyPositive, string value)
            {
                new NumberValidator(precision, scale, onlyPositive).IsValidNumber(value).Should().BeFalse();
            }
          
            [TestCase(20, 5, true, "20.18,8")]
            [TestCase(20, 5, false, "-20.18.18")]
            [TestCase(20, 5, true, "20.180.")]
            public void False_OnMultiDot(int precision, int scale, bool onlyPositive, string value)
            {
                new NumberValidator(precision, scale, onlyPositive).IsValidNumber(value).Should().BeFalse();
            }
          
            [TestCase(20, 5, true, "20,188")]
            [TestCase(20, 5, false, "-20,18")]
            [TestCase(7, 5, true, "20,01234")]
            public void True_OnCommas(int precision, int scale, bool onlyPositive, string value)
            {
                new NumberValidator(precision, scale, onlyPositive).IsValidNumber(value).Should().BeTrue();
            }
          
            [TestCase(20, 5, true, "a.abc")]
            [TestCase(20, 5, false, "-a")]
            [TestCase(20, 5, true, "abc")]
            public void False_OnNonDigits(int precision, int scale, bool onlyPositive, string value)
            {
                new NumberValidator(precision, scale, onlyPositive).IsValidNumber(value).Should().BeFalse();
            }

            [TestCase("0", 17, 2, true)]
            [TestCase("-5", 17, 2, false)]
            [TestCase("+1.3", 4, 2, true)]
            public void True_OnRegularNumbers(string value, int precision, int scale, bool onlyPositive)
            {
                new NumberValidator(precision, scale, onlyPositive).IsValidNumber(value).Should().BeTrue();
            }

            [TestCase("0.0", 3, 2, true)]
            [TestCase("12.345", 5, 3, false)]
            [TestCase("-23.45", 5, 3, false)]
            [TestCase("+120", 5, 0, false)]
            public void True_OnScaleHonored(string value, int precision, int scale, bool onlyPositive)
            {
                new NumberValidator(precision, scale, onlyPositive).IsValidNumber(value).Should().BeTrue();
            }

            [TestCase("1234", 5, 2, true)]
            [TestCase("12.5", 5, 3, false)]
            [TestCase("-2.345", 5, 3, false)]
            [TestCase("-2345", 5, 3, false)]
            [TestCase("+2345", 5, 0, false)]
            public void True_OnPrecisionHonored(string value, int precision, int scale, bool onlyPositive)
            {
                new NumberValidator(precision, scale, onlyPositive).IsValidNumber(value).Should().BeTrue();
            }
        }
    }

	public class NumberValidator
	{
		private readonly Regex numberRegex;
		private readonly bool onlyPositive;
		private readonly int precision;
		private readonly int scale;

		public NumberValidator(int precision, int scale = 0, bool onlyPositive = false)
		{
			this.precision = precision;
			this.scale = scale;
			this.onlyPositive = onlyPositive;
			if (precision <= 0)
				throw new ArgumentException("precision must be a positive number");
			if (scale < 0 || scale >= precision)
				throw new ArgumentException("precision must be a non-negative number less or equal than precision");
			numberRegex = new Regex(@"^([+-]?)(\d+)([.,](\d+))?$", RegexOptions.IgnoreCase);
		}

		public bool IsValidNumber(string value)
		{
			// Проверяем соответствие входного значения формату N(m,k), в соответствии с правилом, 
			// описанным в Формате описи документов, направляемых в налоговый орган в электронном виде по телекоммуникационным каналам связи:
			// Формат числового значения указывается в виде N(m.к), где m – максимальное количество знаков в числе, включая знак (для отрицательного числа), 
			// целую и дробную часть числа без разделяющей десятичной точки, k – максимальное число знаков дробной части числа. 
			// Если число знаков дробной части числа равно 0 (т.е. число целое), то формат числового значения имеет вид N(m).

			if (string.IsNullOrEmpty(value))
				return false;

			var match = numberRegex.Match(value);
			if (!match.Success)
				return false;

			// Знак и целая часть
			var intPart = match.Groups[1].Value.Length + match.Groups[2].Value.Length;
			// Дробная часть
			var fracPart = match.Groups[4].Value.Length;

			if (intPart + fracPart > precision || fracPart > scale)
				return false;

			if (onlyPositive && match.Groups[1].Value == "-")
				return false;
			return true;
		}
	}
}