void GetOutlineSize_float(float Distance, out float OutlineSize)
{
	OutlineSize = 0.0f;

	if (Distance < 2.0f)
	{
		OutlineSize = 2.0f;
	}
	else if (Distance < 10.0f)
	{
		OutlineSize = 1.0f;
	}
	else if (Distance < 40.0f)
	{
		OutlineSize = 0.5f;
	}
}