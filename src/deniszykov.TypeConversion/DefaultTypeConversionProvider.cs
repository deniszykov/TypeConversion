using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace deniszykov.TypeConversion
{
	public class DefaultTypeConversionProvider : ITypeConversionProvider
	{
		private static readonly int ConverterArrayIncrementCount = 20;

		private IConverter[][] typeConverters;
		private readonly MethodInfo getConverterDefinition;
		private readonly Dictionary<long, Func<IConverter>> getConverterByTypes;

		public DefaultTypeConversionProvider()
		{
			this.typeConverters = new IConverter[ConverterArrayIncrementCount][];
			this.getConverterByTypes = new Dictionary<long, Func<IConverter>>();
			this.getConverterDefinition = new Func<IConverter>(this.GetConverter<object, object>).Method.GetGenericMethodDefinition();
		}

		public IConverter<FromType, ToType> GetConverter<FromType, ToType>()
		{
			var fromTypeIndex = ConversionTypeInfo.FromType<FromType>.FromIndex;
			var toTypeIndex = ConversionTypeInfo.FromType<FromType>.ToType<ToType>.ToIndex;
			var toConverters = this.GetToTypeConverters(fromTypeIndex, toTypeIndex);

			if (toConverters[toTypeIndex] is IConverter<FromType, ToType> typeConverter)
			{
				return typeConverter;
			}
			else
			{
				var conversionInfo = GetConversionInfo<FromType, ToType>();
				typeConverter = new Converter<FromType, ToType>(conversionInfo);
				toConverters[toTypeIndex] = typeConverter;
				return typeConverter;
			}
		}
		public IConverter GetConverter(Type fromType, Type toType)
		{
			if (fromType == null) throw new ArgumentNullException(nameof(fromType));
			if (toType == null) throw new ArgumentNullException(nameof(toType));

			var fromHash = fromType.GetHashCode(); // it's not hashcode, it's an unique sync-lock of type-object
			var toHash = toType.GetHashCode();
			var typePairIndex =  unchecked(((long)fromHash << 32) | (uint)toHash);
			var getConverterFunc = default(Func<IConverter>);

			lock (this.getConverterByTypes)
			{
				if (this.getConverterByTypes.TryGetValue(typePairIndex, out getConverterFunc))
				{
					return getConverterFunc();
				}
			}

			var getConverterMethod = getConverterDefinition.MakeGenericMethod(fromType, toType);
			getConverterFunc = (Func<IConverter>)Delegate.CreateDelegate(typeof(Func<IConverter>), this, getConverterMethod);

			lock (this.getConverterByTypes)
			{
				this.getConverterByTypes[typePairIndex] = getConverterFunc;
			}

			return getConverterFunc();
		}

		private IConverter[] GetToTypeConverters(int fromTypeIndex, int toTypeIndex)
		{
			if (fromTypeIndex >= this.typeConverters.Length)
			{
				Array.Resize(ref this.typeConverters, fromTypeIndex + ConverterArrayIncrementCount);
			}

			var toConverters = this.typeConverters[fromTypeIndex];
			while (toConverters == null)
			{
				toConverters = Interlocked.CompareExchange(ref this.typeConverters[fromTypeIndex], new IConverter[ConverterArrayIncrementCount], null);
			}

			while (toTypeIndex >= toConverters.Length)
			{
				var originalToConverters = toConverters;
				Array.Resize(ref toConverters, toConverters.Length + ConverterArrayIncrementCount);
				toConverters = Interlocked.CompareExchange(ref this.typeConverters[fromTypeIndex], toConverters, originalToConverters);
			}

			return toConverters;
		}

		private ConversionInfo GetConversionInfo<FromType, ToType>()
		{
			throw new NotImplementedException();
		}
	}
}
