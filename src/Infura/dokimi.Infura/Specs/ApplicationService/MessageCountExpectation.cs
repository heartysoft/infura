using System;
using System.Text;

namespace dokimi.core.Specs.ApplicationService
{
    public class MessageCount : Expectation
    {
        public static Expectation ShouldBe(int count)
        {
            return new MessageCount(count);
        }

        private readonly int _count;

        public MessageCount(int count)
        {
            _count = count;
        }

        public void DescribeTo(SpecInfo spec, MessageFormatter formatter)
        {
            spec.ReportExpectation(string.Format("There should be {0} new messages.", _count));
        }

        public void VerifyTo(object[] input, SpecInfo results, MessageFormatter formatter)
        {
            if (_count != input.Length)
            {
                results.ReportExpectationFail(string.Format("Expected {0} new messages, but found {1}.", _count, input.Length),
                                              new MessageCountMismatchException(_count, input, formatter));
            }
            else
            {
                results.ReportExpectationPass(string.Format("The correct number of messages ({0}) were generated.",
                                                            _count));
            }
        }
    }

    public class MessageCountMismatchException : Exception
    {

        public MessageCountMismatchException(int count, object[] input, MessageFormatter formatter)
            : base(getMessage(count, input, formatter))
        {
        }

        private static string getMessage(int count, object[] inputs, MessageFormatter formatter)
        {
            var sb = new StringBuilder();

            sb.AppendFormat("Expected {0} messages, but the following {1} were generated:", count, inputs.Length);
            sb.AppendLine();

            foreach (var input in inputs)
            {
                sb.AppendLine(formatter.FormatMessage(input));
            }

            sb.AppendLine("--End of expected messages--");

            return sb.ToString();
        }
    }
}