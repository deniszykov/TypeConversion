using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace deniszykov.BaseN.Benchmark
{
	[MemoryDiagnoser]
	public class BaseNEncodingTest
	{
		private const int N = 20 * 1024 * 1024;
		private readonly byte[] data;

		public BaseNEncodingTest()
		{
			data = new byte[N];
			new Random(42).NextBytes(data);
		}

		[Benchmark(Baseline = true)]
		public void Base64_Encode_ByteArray_Char()
		{
			var encoder = new BaseNDecoder(BaseNAlphabet.Base64Alphabet);
			var output = new char[4 * 1024];
			for (var i = 0; i < data.Length;)
			{
				var flush = this.data.Length - i <= BaseNAlphabet.Base64Alphabet.EncodingBlockSize;
				encoder.Convert(this.data, i, this.data.Length - i, output, 0, output.Length, flush, out var inputUsed, out var outputUsed, out var complete);
				i += inputUsed;
			}
		}
		[Benchmark]
		public void Base64_Encode_ByteArray_Byte()
		{
			var encoder = new BaseNDecoder(BaseNAlphabet.Base64Alphabet);
			var output = new byte[4 * 1024];
			for (var i = 0; i < data.Length;)
			{
				var flush = this.data.Length - i <= BaseNAlphabet.Base64Alphabet.EncodingBlockSize;
				encoder.Convert(this.data, i, this.data.Length - i, output, 0, output.Length, flush, out var inputUsed, out var outputUsed, out var complete);
				i += inputUsed;
			}
		}
		[Benchmark]
		public unsafe void Base64_Encode_Ptr_Char()
		{
			var encoder = new BaseNDecoder(BaseNAlphabet.Base64Alphabet);
			var output = new char[4 * 1024];

			fixed (byte* inputPtr = this.data)
			fixed (char* outputPtr = output)
				for (var i = 0; i < data.Length;)
				{
					var flush = this.data.Length - i <= BaseNAlphabet.Base64Alphabet.EncodingBlockSize;
					encoder.Convert(inputPtr + i, this.data.Length - i, outputPtr, output.Length, flush, out var inputUsed, out var outputUsed, out var complete);
					i += inputUsed;
				}
		}
		[Benchmark]
		public unsafe void Base64_Encode_Ptr_Byte()
		{
			var encoder = new BaseNDecoder(BaseNAlphabet.Base64Alphabet);
			var output = new byte[4 * 1024];

			fixed (byte* inputPtr = this.data)
			fixed (byte* outputPtr = output)
				for (var i = 0; i < data.Length;)
				{
					var flush = this.data.Length - i <= BaseNAlphabet.Base64Alphabet.EncodingBlockSize;
					encoder.Convert(inputPtr + i, this.data.Length - i, outputPtr, output.Length, flush, out var inputUsed, out var outputUsed, out var complete);
					i += inputUsed;
				}
		}
#if NETCOREAPP
		[Benchmark]
		public void Base64_Encode_Span_Char()
		{
			var encoder = new BaseNDecoder(BaseNAlphabet.Base64Alphabet);
			var inputSpan = this.data.AsSpan();
			var output = new char[4 * 1024];
			var outputSpan = output.AsSpan();

			for (var i = 0; i < data.Length;)
			{
				var flush = this.data.Length - i <= BaseNAlphabet.Base64Alphabet.EncodingBlockSize;
				encoder.Convert(inputSpan.Slice(i), outputSpan, flush, out var inputUsed, out var outputUsed, out var complete);
				i += inputUsed;
			}
		}
		[Benchmark]
		public void Base64_Encode_Span_Byte()
		{
			var encoder = new BaseNDecoder(BaseNAlphabet.Base64Alphabet);
			var inputSpan = this.data.AsSpan();
			var output = new byte[4 * 1024];
			var outputSpan = output.AsSpan();

			for (var i = 0; i < data.Length;)
			{
				var flush = this.data.Length - i <= BaseNAlphabet.Base64Alphabet.EncodingBlockSize;
				encoder.Convert(inputSpan.Slice(i), outputSpan, flush, out var inputUsed, out var outputUsed, out var complete);
				i += inputUsed;
			}
		}
#endif
	}

	class Program
	{
		static void Main(string[] args)
		{
			var summary = BenchmarkRunner.Run<BaseNEncodingTest>();
			Console.WriteLine(summary.ToString());
			Console.ReadKey();
		}
	}
}
