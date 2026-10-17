using System;
using System.Collections.Generic;
using System.Text;

namespace QuiteEnoughRecipes.ModAPI;

public abstract class Position<T>;

public sealed class Before<T> : Position<T>
{
	public T Value { get; }
	public Before(T value)
	{
		Value = value;
	}
}

public sealed class After<T> : Position<T>
{
	public T Value { get; }
	public After(T value)
	{
		Value = value;
	}
}
