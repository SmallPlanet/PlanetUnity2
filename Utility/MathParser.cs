/* 
 * Copyright (C) 2012 Artem Los,
 * All rights reserved.
 * 
 * Please see the license file in the project folder,
 * or, goto https://mathosparser.codeplex.com/license.
 * 
 * Please feel free to ask me directly at my email!
 *  artem@artemlos.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using UnityEngine;

namespace Mathos.Parser
{
    /// <summary>
    /// This is a mathematical expression parser that allows you to parser a string value,
    /// perform the required calculations, and return a value in form of a decimal.
    /// </summary>
    public class MathParser
    {

		static public float step(float a, float b){
			return Mathf.Floor (a / Mathf.Floor (a / b));
		}

        /// <summary>
        /// This constructor will add some basic operators, functions, and variables
        /// to the parser. Please note that you are able to change that using
        /// boolean flags
        /// </summary>
        /// <param name="loadPreDefinedFunctions">This will load "abs", "cos", "cosh", "arccos", "sin", "sinh", "arcsin", "tan", "tanh", "arctan", "sqrt", "rem", "round"</param>
        /// <param name="loadPreDefinedOperators">This will load "%", "*", ":", "/", "+", "-", ">", "&lt;", "="</param>
        /// <param name="loadPreDefinedVariables">This will load "pi" and "e"</param>
        public MathParser(bool loadPreDefinedFunctions = true, bool loadPreDefinedOperators = true, bool loadPreDefinedVariables = true)
        {
            if (loadPreDefinedOperators)
            {
                // by default, we will load basic arithmetic operators.
                // please note, its possible to do it either inside the constructor,
                // or outside the class. the lowest value will be executed first!
                OperatorList.Add("%"); // modulo
                OperatorList.Add("^"); // to the power of
                OperatorList.Add(":"); // division 1
                OperatorList.Add("/"); // division 2
                OperatorList.Add("*"); // multiplication
                OperatorList.Add("-"); // subtraction
                OperatorList.Add("+"); // addition

                OperatorList.Add(">"); // greater than
                OperatorList.Add("<"); // less than
                OperatorList.Add("="); // are equal


                // when an operator is executed, the parser needs to know how.
                // this is how you can add your own operators. note, the order
                // in this list does not matter.
                _operatorAction.Add("%", (numberA, numberB) => numberA % numberB);
                _operatorAction.Add("^", (numberA, numberB) => (decimal)Math.Pow((double)numberA, (double)numberB));
                _operatorAction.Add(":", (numberA, numberB) => numberA / numberB);
                _operatorAction.Add("/", (numberA, numberB) => numberA / numberB);
                _operatorAction.Add("*", (numberA, numberB) => numberA * numberB);
                _operatorAction.Add("+", (numberA, numberB) => numberA + numberB);
                _operatorAction.Add("-", (numberA, numberB) => numberA - numberB);

                _operatorAction.Add(">", (numberA, numberB) => numberA > numberB ? 1 : 0);
                _operatorAction.Add("<", (numberA, numberB) => numberA < numberB ? 1 : 0);
                _operatorAction.Add("=", (numberA, numberB) => numberA == numberB ? 1 : 0);
            }


            if (loadPreDefinedFunctions)
            {
                // these are the basic functions you might be able to use.
                // as with operators, localFunctions might be adjusted, i.e.
                // you can add or remove a function.
                // please open the "MathosTest" project, and find MathParser.cs
                // in "CustomFunction" you will see three ways of adding 
                // a new function to this variable!
                // EACH FUNCTION MAY ONLY TAKE ONE PARAMETER, AND RETURN ONE
                // VALUE. THESE VALUES SHOULD BE IN "DECIMAL FORMAT"!
                LocalFunctions.Add("abs", x => (decimal)Math.Abs((double)x[0]));

                LocalFunctions.Add("cos", x => (decimal)Math.Cos((double)x[0]));
                LocalFunctions.Add("cosh", x => (decimal)Math.Cosh((double)x[0]));
                LocalFunctions.Add("arccos", x => (decimal)Math.Acos((double)x[0]));

                LocalFunctions.Add("sin", x => (decimal)Math.Sin((double)x[0]));
                LocalFunctions.Add("sinh", x => (decimal)Math.Sinh((double)x[0]));
                LocalFunctions.Add("arcsin", x => (decimal)Math.Asin((double)x[0]));

                LocalFunctions.Add("tan", x => (decimal)Math.Tan((double)x[0]));
                LocalFunctions.Add("tanh", x => (decimal)Math.Tanh((double)x[0]));
                LocalFunctions.Add("arctan", x => (decimal)Math.Atan((double)x[0]));
                //LocalFunctions.Add("arctan2", x => (decimal)Math.Atan2((double)x[0], (double)x[1]));

                LocalFunctions.Add("sqrt", x => (decimal)Math.Sqrt((double)x[0]));
                LocalFunctions.Add("rem", x => (decimal)Math.IEEERemainder((double)x[0], (double)x[1]));
                LocalFunctions.Add("root", x => (decimal)Math.Pow((double)x[0], 1.0 / (double)x[1]));

                LocalFunctions.Add("pow", x => (decimal)Math.Pow((double)x[0], (double)x[1]));


				LocalFunctions.Add("step", (x) => {
					// the step function divide the first number (total width) by the second number (min width)
					// example: step(1024, 200) would result in 204.8
					// example: step(1024, 300) would result in 341.3

					return (decimal)step((float)x[0], (float)x[1]);
				});

				LocalFunctions.Add("max", x => (decimal)Math.Max((double)x[0], (double)x[1]));
				LocalFunctions.Add("min", x => (decimal)Math.Min((double)x[0], (double)x[1]));


                LocalFunctions.Add("exp", x => (decimal)Math.Exp((double)x[0]));
                //LocalFunctions.Add("log", x => (decimal)Math.Log((double)x[0]));
                //LocalFunctions.Add("log10", x => (decimal)Math.Log10((double)x[0]));
                LocalFunctions.Add("log", delegate(decimal[] input)
                {
                    // input[0] is the number
                    // input[1] is the base

                    if (input.Length == 1)
                    {
                        return (decimal)Math.Log((double)input[0]);
                    }
                    else if (input.Length == 2)
                    {
                        return (decimal)Math.Log((double)input[0], (double)input[1]);
                    }
                    else
                    {
                        return 0; // false
                    }
                });

                LocalFunctions.Add("round", x => (decimal)Math.Round((double)x[0]));
                LocalFunctions.Add("truncate", x => (decimal)Math.Truncate((double)x[0]));
                LocalFunctions.Add("floor", x => (decimal)Math.Floor((double)x[0]));
                LocalFunctions.Add("ceiling", x => (decimal)Math.Ceiling((double)x[0]));
                LocalFunctions.Add("sign", x => (decimal)Math.Sign((double)x[0]));

            }

            if (loadPreDefinedVariables)
            {
                // local variables such as pi can also be added into the parser.
                LocalVariables.Add("pi", (decimal)3.14159265358979323846264338327950288); // the simplest variable!
                LocalVariables.Add("pi2", (decimal)6.28318530717958647692528676655900576);
                LocalVariables.Add("pi05", (decimal)1.57079632679489661923132169163975144);
                LocalVariables.Add("pi025", (decimal)0.78539816339744830961566084581987572);
                LocalVariables.Add("pi0125", (decimal)0.39269908169872415480783042290993786);
                LocalVariables.Add("pitograd", (decimal)57.2957795130823208767981548141051704);
                LocalVariables.Add("piofgrad", (decimal)0.01745329251994329576923690768488612);

                LocalVariables.Add("e", (decimal)2.71828182845904523536028747135266249);
                LocalVariables.Add("phi", (decimal)1.61803398874989484820458683436563811);
                LocalVariables.Add("major", (decimal)0.61803398874989484820458683436563811);
                LocalVariables.Add("minor", (decimal)0.38196601125010515179541316563436189); 
            }

        }


        /* THE LOCAL VARIABLES */
        private List<string> _operatorList = new List<string>();
        /// <summary>
        /// All operators should be inside this property.
        /// The first operator is executed first, et cetera.
        /// An operator may only be ONE character.
        /// </summary>
        public List<string> OperatorList
        {
            get { return _operatorList; }
            set { _operatorList = value; }
        }
        private Dictionary<string, Func<decimal, decimal, decimal>> _operatorAction = new Dictionary<string, Func<decimal, decimal, decimal>>();
        /// <summary>
        /// When adding a variable in the OperatorList property, you need to assign how that operator should work.
        /// </summary>
        public Dictionary<string, Func<decimal, decimal, decimal>> OperatorAction
        {
            get { return _operatorAction; }
            set { _operatorAction = value; }
        }
        private Dictionary<string, Func<decimal[], decimal>> _localFunctions = new Dictionary<string, Func<decimal[], decimal>>();
        /// <summary>
        /// All functions that you want to define should be inside this property.
        /// </summary>
        public Dictionary<string, Func<decimal[], decimal>> LocalFunctions
        {
            get { return _localFunctions; }
            set { _localFunctions = value; }
        }
        private Dictionary<string, decimal> _localVariables = new Dictionary<string, decimal>();
        /// <summary>
        /// All variables that you want to define should be inside this property.
        /// </summary>
        public Dictionary<string, decimal> LocalVariables
        {
            get { return _localVariables; }
            set { _localVariables = value; }
        }

        private CultureInfo _CULTURE_INFO = CultureInfo.InvariantCulture;
        /// <summary>
        /// When converting the result from the Parse method or ProgrammaticallyParse method ToString(),
        /// please use this cultur info.
        /// </summary>
        public CultureInfo CULTURE_INFO
        {
            get { return _CULTURE_INFO; }
        }


        /* PARSER FUNCTIONS, PUBLIC */
        /// <summary>
        /// Enter the math expression in form of a string.
        /// </summary>
        /// <param name="mathExpression"></param>
        /// <returns></returns>
        public decimal Parse(string mathExpression)
        {
            return Scanner(_expr: mathExpression);
        }

        /// <summary>
        /// Enter the math expression in form of a string. You might also add/edit variables using "let" keyword.
        /// For example, "let sampleVariable = 2+2".
        /// 
        /// Another way of adding/editing a variable is to type "varName := 20"
        /// 
        /// Last way of adding/editing a variable is to type "let varName be 20"
        /// </summary>
        /// <param name="mathExpression"></param>
        /// <returns></returns>
        public decimal ProgrammaticallyParse(string mathExpression, bool correctExpression = true, bool identifyComments = true)
        {
            if (identifyComments)
            {
                mathExpression = System.Text.RegularExpressions.Regex.Replace(mathExpression, "#\\{.*?\\}#", ""); // Delete Comments #{Comment}#
                mathExpression = System.Text.RegularExpressions.Regex.Replace(mathExpression, "#.*$", ""); // Delete Comments #Comment
            }

            if (correctExpression)
            {
                mathExpression = Correction(mathExpression); // this refers to the Correction function which will correct stuff like artn to arctan, etc.
            }

            if (mathExpression.Contains("let"))
            {
                string varName;
                if (mathExpression.Contains("be"))
                {
                    varName = mathExpression.Substring(mathExpression.IndexOf("let") + 3, mathExpression.IndexOf("be") - mathExpression.IndexOf("let") - 3);
                    mathExpression = mathExpression.Replace(varName + "be", "");
                }
                else
                {
                    varName = mathExpression.Substring(mathExpression.IndexOf("let") + 3, mathExpression.IndexOf("=") - mathExpression.IndexOf("let") - 3);
                    mathExpression = mathExpression.Replace(varName + "=", "");
                }

                varName = varName.Replace(" ", "");
                mathExpression = mathExpression.Replace("let", "");
                decimal varValue = Parse(mathExpression);

                if (LocalVariables.ContainsKey(varName))
                {
                    LocalVariables[varName] = varValue;
                }
                else
                {
                    LocalVariables.Add(varName, varValue);
                }
                return varValue;
            }
            else if (mathExpression.Contains(":="))
            {
                //mathExpression = mathExpression.Replace(" ", ""); // remove white space
                string varName = mathExpression.Substring(0, mathExpression.IndexOf(":="));
                mathExpression = mathExpression.Replace(varName + ":=", "");

                decimal varValue = Parse(mathExpression);
                varName = varName.Replace(" ", "");
                if (LocalVariables.ContainsKey(varName))
                {
                    LocalVariables[varName] = varValue;
                }
                else
                {
                    LocalVariables.Add(varName, varValue);
                }

                return varValue;
            }
            else
            {
                return Parse(mathExpression);
            }
        }

        private string Correction(string input)
        {
            // Word corrections

            input =System.Text.RegularExpressions. Regex.Replace(input, "\\b(sqr|sqrt)\\b", "sqrt", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            input = System.Text.RegularExpressions.Regex.Replace(input, "\\b(atan2|arctan2)\\b", "arctan2", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            //... and more

            return input;
        }


        /* UNDER THE HOOD - THE CORE OF THE PARSER */
        private decimal Scanner(string _expr)
        {
            // SCANNING THE INPUT STRING AND CONVERT IT INTO TOKENS

            List<string> _tokens = new List<string>();
            string _vector = "";

            //_expr = _expr.Replace(" ", ""); // remove white space
            _expr = _expr.Replace("+-", "-"); // some basic arithmetical rules
            _expr = _expr.Replace("-+", "-");
            _expr = _expr.Replace("--", "+");

           /* foreach (var item in LocalVariables)
            {
                // replace the current variables with their value
                _expr = _expr.Replace(item.Key, "(" + item.Value.ToString(CULTURE_INFO) + ")");
            }*/

            for (int i = 0; i < _expr.Length; i++)
            {
                char ch = _expr[i];

                if (char.IsWhiteSpace(ch)) { } // could also be used to remove white spaces.
                else if (Char.IsLetter(ch))
                {
                    if (i != 0 && (Char.IsDigit(_expr[i - 1]) || Char.IsDigit(_expr[i - 1]) || _expr[i - 1] == ')'))
                    {
                        _tokens.Add("*");
                    }

                    _vector = _vector + ch;
                    while ((i + 1) < _expr.Length && Char.IsLetterOrDigit(_expr[i + 1])) // here is it is possible to choose whether you want variables that only contain letters with or without digits.
                    {
                        i++;
                        _vector = _vector + _expr[i];
                    }
                    if (_vector != null)
                    {
                        _tokens.Add(_vector);
                        _vector = "";
                    }
                }
                else if (Char.IsDigit(ch))
                {
                    _vector = _vector + ch;
                    while ((i + 1) < _expr.Length && (Char.IsDigit(_expr[i + 1]) || _expr[i + 1] == '.')) // removed || _expr[i + 1] == ','
                    {
                        i++;
                        _vector = _vector + _expr[i];
                    }
                    if (_vector != null)
                    {
                        _tokens.Add(_vector);
                        _vector = "";
                    }
                }
                else if ((i + 1) < _expr.Length && (ch == '-' || ch == '+') && Char.IsDigit(_expr[i + 1]) && (i == 0 || OperatorList.IndexOf(_expr[i - 1].ToString()) != -1 || ((i - 1) > 0 && _expr[i - 1] == '(')))
                {
                    // if the above is true, then, the token for that negative number will be "-1", not "-","1".
                    // to sum up, the above will be true if the minus sign is in front of the number, but
                    // at the beginning, for example, -1+2, or, when it is inside the brakets (-1).
                    // NOTE: this works for + sign as well!
                    _vector = _vector + ch;
                    while ((i + 1) < _expr.Length && (Char.IsDigit(_expr[i + 1]) || _expr[i + 1] == '.')) // removed || _expr[i + 1] == ','
                    {
                        i++;
                        _vector = _vector + _expr[i];
                    }
                    if (_vector != null)
                    {
                        _tokens.Add(_vector);
                        _vector = "";
                    }
                }
                else if (ch == '(')
                {
                    if (i != 0 && (Char.IsDigit(_expr[i - 1]) || Char.IsDigit(_expr[i - 1]) || _expr[i - 1] == ')'))
                    {
                        _tokens.Add("*"); // if we remove this line, we would be able to have numbers in function names. however, then we can't parser 3(2+2)
                        _tokens.Add("(");
                    }
                    else
                    {
                        _tokens.Add("(");
                    }
                }
                else
                {
                    _tokens.Add(ch.ToString());
                }
            }

            return MathParserLogic(_tokens);
        }

        private decimal MathParserLogic(List<string> _tokens)
        {
            // CALCULATING THE EXPRESSIONS INSIDE THE BRACKETS
            // IF NEEDED, EXECUTE A FUNCTION

            // Variables replacement
            for (int i = 0; i < _tokens.Count; i++)
            {
                if (LocalVariables.Keys.Contains(_tokens[i]))
                {
                    _tokens[i] = LocalVariables[_tokens[i]].ToString();
                }
            }
            while (_tokens.IndexOf("(") != -1)
            {
                // getting data between "(", ")"
                int open = _tokens.LastIndexOf("(");
                int close = _tokens.IndexOf(")", open); // in case open is -1, i.e. no "(" // , open == 0 ? 0 : open - 1
                if (open  >= close)
                {
                    // if there is no closing bracket, throw a new exception
                    throw new ArithmeticException("No closing bracket/parenthesis! tkn: " + open.ToString());
                }
                List<string> roughExpr = new List<string>();
                for (int i = open + 1; i < close; i++)
                {
                    roughExpr.Add(_tokens[i]);
                }

                decimal result = 0; // the temporary result is stored here

                string functioName = _tokens[open == 0 ? 0 : open - 1];
                decimal[] _args = new decimal[0];
                if (LocalFunctions.Keys.Contains(functioName))
                {
                    if (roughExpr.Contains(","))
                    {
                        // converting all arguments into a decimal array
                        for (int i = 0; i < roughExpr.Count; i++)
                        {
                            int firstCommaOrEndOfExpression = roughExpr.IndexOf(",", i) != -1 ? roughExpr.IndexOf(",", i) : roughExpr.Count;

                            List<string> defaultExpr = new List<string>();
                            while (i < firstCommaOrEndOfExpression)
                            {
                                defaultExpr.Add(roughExpr[i]);
                                i++;
                            }

                            // changing the size of the array of arguments
                            Array.Resize(ref _args, _args.Length + 1);
                            if (defaultExpr.Count == 0)
                            {
                                _args[_args.Length - 1] = 0;
                            }
                            else
                            {
                                _args[_args.Length - 1] = BasicArithmeticalExpression(defaultExpr);
                            }
                        }

                        // finnaly, passing the arguments to the given function
                        result = decimal.Parse(LocalFunctions[functioName](_args).ToString(CULTURE_INFO), CULTURE_INFO);
                    }

                    else
                    {
                        // but if we only have one argument, then we pass it directly to the function
                        result = decimal.Parse(LocalFunctions[functioName](new Decimal[] { BasicArithmeticalExpression(roughExpr) }).ToString(CULTURE_INFO), CULTURE_INFO);
                    }
                }
                else
                {
                    // if no function is need to execute following expression, pass it
                    // to the "BasicArithmeticalExpression" method.
                    result = BasicArithmeticalExpression(roughExpr);
                }

                // when all the calculations have been done
                // we replace the "opening bracket with the result"
                // and removing the rest.
                _tokens[open] = result.ToString(CULTURE_INFO);
                _tokens.RemoveRange(open + 1, close - open);
                if (LocalFunctions.Keys.Contains(functioName))
                {
                    // if we also executed a function, removing
                    // the function name as well.
                    _tokens.RemoveAt(open - 1);
                }
            }

            // at this point, we should have replaced all brackets
            // with the appropriate values, so we can simply
            // calculate the expression. it's not so complex
            // any more!
            return BasicArithmeticalExpression(_tokens);
        }

        private decimal BasicArithmeticalExpression(List<string> _tokens)
        {
            // PERFORMING A BASIC ARITHMETICAL EXPRESSION CALCULATION
            // THIS METHOD CAN ONLY OPERATE WITH NUMBERS AND OPERATORS
            // AND WILL NOT UNDERSTAND ANYTHING BEYOND THAT.

            if (_tokens.Count == 1)
            {
                return decimal.Parse(_tokens[0], CULTURE_INFO);
            }
            else if (_tokens.Count == 2)
            {
                string op = _tokens[0];
                if (op == "-" || op == "+")
                {
                    return decimal.Parse((op == "+" ? "" : "-") + _tokens[1], CULTURE_INFO);
                }
                else
                {
                    return OperatorAction[op](0, decimal.Parse(_tokens[1], CULTURE_INFO));
                }
            }
            else if (_tokens.Count == 0)
            {
                return 0;
            }
            foreach (string op in OperatorList)
            {
                while (_tokens.IndexOf(op) != -1)
                {
                    int opPlace = _tokens.IndexOf(op);

                    decimal numberA = Convert.ToDecimal(_tokens[opPlace - 1], CULTURE_INFO);
                    decimal numberB = Convert.ToDecimal(_tokens[opPlace + 1], CULTURE_INFO);

                    decimal result = OperatorAction[op](numberA, numberB);

                    _tokens[opPlace - 1] = result.ToString(CULTURE_INFO);
                    _tokens.RemoveRange(opPlace, 2);
                }
            }
            return Convert.ToDecimal(_tokens[0], CULTURE_INFO);
        }
    }

}