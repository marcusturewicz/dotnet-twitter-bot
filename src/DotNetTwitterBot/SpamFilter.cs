using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Collections.Generic;
using System.Linq;

namespace DotNetTwitterBot
{
    public class SpamFilter
    {
        private readonly InferenceSession _session;
        private readonly string _inputName = "input";
        private readonly string _outputName = "output_probability";
        private readonly int _spamIndex = 1;
        private readonly float _spamThreshold = 0.5f;

        public SpamFilter(string modelPath)
        {
            _session = new InferenceSession(modelPath);
        }

        public IEnumerable<bool> Run(IEnumerable<string> textList)
        {
            var textArray = textList.ToArray();
            var count = textArray.Length;

            var input = new DenseTensor<string>(new[] { count, 1 });

            for (var i = 0; i < count; i++)
            {
                input.SetValue(i, textArray[i]);
            }

            var inputs = new List<NamedOnnxValue>()
            {
                NamedOnnxValue.CreateFromTensor(_inputName, input)
            };

            var outputs = new List<string> { _outputName };

            var results = _session.Run(inputs, outputs).ToArray()[0].Value as IList<DisposableNamedOnnxValue>;

            return results.Select(x => (x.Value as IDictionary<long, float>)[_spamIndex] > _spamThreshold);
        }
    }
}
