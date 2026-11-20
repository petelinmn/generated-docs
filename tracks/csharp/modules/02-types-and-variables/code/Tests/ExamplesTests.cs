using System.Reflection;
using Examples;

namespace Tests;

public class Ex01_CopySemanticsTests
{
    [Fact]
    public void StructAssignment_CopiesTheValue() =>
        Assert.Equal((1, 99), CopySemantics.MutateStructCopy());

    [Fact]
    public void ClassAssignment_SharesTheObject() =>
        Assert.Equal((99, 99), CopySemantics.MutateClassCopy());

    [Fact]
    public void Arguments_FollowTheSameRule()
    {
        var structPoint = new PointStruct { X = 5 };
        var classPoint = new PointClass { X = 5 };

        CopySemantics.Reset(structPoint);
        CopySemantics.Reset(classPoint);

        Assert.Equal(5, structPoint.X);
        Assert.Equal(0, classPoint.X);
    }
}

public class Ex02_ConversionsTests
{
    [Fact]
    public void ExplicitCast_TruncatesTowardZero()
    {
        Assert.Equal(2, Conversions.Truncate(2.9));
        Assert.Equal(-2, Conversions.Truncate(-2.9));
    }

    [Fact]
    public void Overflow_WrapsUnlessChecked()
    {
        Assert.Equal(int.MinValue, Conversions.AddUnchecked(int.MaxValue, 1));
        Assert.Throws<OverflowException>(() => Conversions.AddChecked(int.MaxValue, 1));
        Assert.Equal(3_000_000_000L, Conversions.Widen(int.MaxValue) + Conversions.Widen(852_516_353));
    }

    [Fact]
    public void TryParse_ReturnsNullOnBadInput()
    {
        Assert.Equal(42, Conversions.ParseOrNull("42"));
        Assert.Null(Conversions.ParseOrNull("4x2"));
    }

    [Fact]
    public void Round_IsBankersByDefault()
    {
        Assert.Equal(2, Conversions.RoundDefault(2.5));
        Assert.Equal(4, Conversions.RoundDefault(3.5));
        Assert.Equal(3, Conversions.RoundAwayFromZero(2.5));
    }
}

public class Ex03_StringsTests
{
    [Fact]
    public void Strings_AreImmutable() =>
        Assert.Equal(("hello", "HELLO"), Strings.Immutability());

    [Fact]
    public void Interpolation_UsesInvariantCulture() =>
        Assert.Equal("Tea: 3.50", Strings.PriceTag("Tea", 3.5m));

    [Fact]
    public void VerbatimAndRawLiterals_NeedNoEscaping()
    {
        Assert.Equal("C:\\temp\\new", Strings.VerbatimPath);
        Assert.Equal("{\"name\": \"Alice\", \"path\": \"C:\\temp\"}", Strings.RawJson);
    }

    [Fact]
    public void StringBuilder_BuildsCsv() =>
        Assert.Equal("0,1,2,3,4", Strings.NumbersCsv(5));
}

public class Ex04_NullHandlingTests
{
    [Fact]
    public void NullableInt_HoldsNoValueWhenNotFound()
    {
        Assert.Equal(1, NullHandling.FindIndex([10, 20, 30], 20));
        Assert.Null(NullHandling.FindIndex([10, 20, 30], 99));
    }

    [Fact]
    public void NullOperators_GiveFallbacks()
    {
        Assert.Equal(0, NullHandling.LengthOrZero(null));
        Assert.Equal(3, NullHandling.LengthOrZero("abc"));
        Assert.Equal("Anonymous", NullHandling.EnsureName(null));
        Assert.Equal("Bob", NullHandling.EnsureName("Bob"));
        Assert.Equal("", NullHandling.Shout(null));
        Assert.Equal("HI", NullHandling.Shout("hi"));
    }
}

public class Ex05_BoxingTests
{
    [Fact]
    public void TwoBoxes_AreDifferentObjectsWithEqualValues() =>
        Assert.Equal((false, true), Boxing.CompareBoxes());

    [Fact]
    public void EqualityOperator_ComparesReferences() =>
        Assert.False(Boxing.EqualityOperatorOnBoxes());

    [Fact]
    public void Unboxing_RequiresTheExactType()
    {
        Assert.Throws<InvalidCastException>(() => Boxing.UnboxAsLong(42));
        Assert.Equal(42L, Boxing.UnboxAsLong(42L));
    }
}

public class Ex06_FloatingPointTests
{
    [Fact]
    public void Double_IsInexact_DecimalIsExact()
    {
        Assert.False(FloatingPoint.DoubleSumIsExact());
        Assert.True(FloatingPoint.DecimalSumIsExact());
        Assert.Equal("0.30000000000000004", FloatingPoint.DoubleSumText());
        Assert.True(FloatingPoint.NearlyEqual(0.1 + 0.2, 0.3));
    }

    [Fact]
    public void DoubleDivisionByZero_IsInfinity() =>
        Assert.Equal(double.PositiveInfinity, FloatingPoint.DivideByZero(1.0));
}

public class Ex07_CultureAwareTests
{
    [Fact]
    public void Casing_DependsOnCulture()
    {
        Assert.Equal("\u0130", CultureAware.ToUpperTurkish("i"));
        Assert.Equal("I", CultureAware.ToUpperInvariant("i"));
        Assert.True(CultureAware.EqualsIgnoreCase("FILE.txt", "file.TXT"));
    }

    [Fact]
    public void NumberFormat_DependsOnCulture()
    {
        Assert.Equal("1.234,50", CultureAware.FormatInGerman(1234.5));
        Assert.Equal("1,234.50", CultureAware.FormatInvariant(1234.5));
    }
}

public class Ex08_LayoutTests
{
    [Fact]
    public void Padding_AlignsFields()
    {
        Assert.Equal(8, Layout.SizeOfPadded);
        Assert.Equal(5, Layout.SizeOfPacked);
        Assert.Equal(IntPtr.Size, Layout.SizeOfReference);
    }
}

public class Ex09_InterningTests
{
    [Fact]
    public void OnlyCompileTimeStrings_AreInternedAutomatically()
    {
        Assert.True(Interning.LiteralsShareInstance());
        Assert.False(Interning.RuntimeConcatSharesInstance("hel"));
        Assert.True(Interning.InternedConcatSharesInstance("hel"));
    }
}

public class Ex10_NullableInternalsTests
{
    [Fact]
    public void BoxingNullable_BoxesUnderlyingValueOrNull()
    {
        Assert.Equal(typeof(int), NullableInternals.BoxedTypeOf(5));
        Assert.Null(NullableInternals.BoxedTypeOf(null));
    }

    [Fact]
    public void NullableReferenceAnnotation_IsMetadataOnly()
    {
        Assert.Equal(typeof(string), NullableInternals.ParameterType(nameof(NullableInternals.TakesNullable)));
        Assert.Equal(typeof(string), NullableInternals.ParameterType(nameof(NullableInternals.TakesNonNullable)));
        Assert.Equal(NullabilityState.Nullable, NullableInternals.ParameterNullability(nameof(NullableInternals.TakesNullable)));
        Assert.Equal(NullabilityState.NotNull, NullableInternals.ParameterNullability(nameof(NullableInternals.TakesNonNullable)));
    }
}
