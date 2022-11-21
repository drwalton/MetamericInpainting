int outOfBounds(float2 uv, float w, float h, float ts) {
	if (uv.x > w / ts + 0.1 || uv.y > h / ts + 0.1)
		return 1;
	else
		return 0;
}

float srgb2Linear(float s)
{
	if (s < 0.04045) return s / 12.92;
	else return pow((s + 0.055) / 1.055, 2.4);
}
float linear2Srgb(float l)
{
	if (l < 0.0031308) return l * 12.92;
	else return 1.055 * pow(l, 1.0 / 2.4) - 0.055;
}


float3 srgb2Linear(float3 s)
{
	return float3(srgb2Linear(s.x), srgb2Linear(s.y), srgb2Linear(s.z));
}

float3 linear2Srgb(float3 l)
{
	return float3(linear2Srgb(l.x), linear2Srgb(l.y), linear2Srgb(l.z));
}

float calculateFoveationLOD(float2 uv,float alpha, float foveaSize,float2 foveaXY = float2(0.5,0.5)) 
{
	if (foveaSize < 0) return alpha;
	float dist = distance(uv, foveaXY) - foveaSize;
	if (dist < 0) dist = 0;
	float foveation = 0;
	if(foveaSize == 0.021)
		foveation = alpha * dist / sqrt(0.5);
	else
		foveation =  alpha * pow(dist,2) / 0.5;

	foveation = foveation > alpha ? alpha : foveation;
	return foveation;
}


float calculateFoveationRect(float2 uv, float alpha, float foveaSize, float2 screenSize, float2 foveaXY = float2(0.5, 0.5))
{
	int size = (int) max(screenSize.x,screenSize.y);
#if SHADER_API_GLES3
	int s = 1;
	while (s < size) {
		s <<= 2;
	}
	size = s;
#else
	//Functions below don't work on webgl
	size = 2 << firstbithigh(size);
	//if screen is square, we dont need the next power of two, just the current.
	if (countbits(screenSize.x) == 1 && countbits(screenSize.y)==1) {
		size = size >> 1;
	}
#endif
	screenSize.x = screenSize.x / size;
	screenSize.y = screenSize.y / size;

	float diag = sqrt(pow(screenSize.x, 2) + pow(screenSize.y, 2))/2;

	if (foveaSize < 0) return alpha;
	float dist = distance(uv, foveaXY) - foveaSize;
	if (dist < 0) dist = 0;
	float foveation = alpha * pow(dist / diag, 2) ;

	foveation = foveation > alpha ? alpha : foveation;
	return foveation;
}

float calculateFoveationBlending(float2 uv, float foveaSize, float2 foveaXY = float2(0.5, 0.5))
{
	float blendstart = 0.75;

	float diff = distance(uv, foveaXY);
	if (diff > foveaSize)
		return 0;
	if (diff < foveaSize*blendstart)
		return 1;
	
	return 1 - ((diff - (foveaSize * blendstart)) /
			  (foveaSize - (foveaSize * blendstart)));

}

// using http://www.easyrgb.com/index.php?X=MATH&H=01#text1

	float3 rgb2lab(float3 rgb)
	{
		float var_R = rgb.x;
		float var_G = rgb.y;
		float var_B = rgb.z;


		if (var_R > 0.04045) var_R = pow(((var_R + 0.055) / 1.055), 2.4);
		else                   var_R = var_R / 12.92;
		if (var_G > 0.04045) var_G = pow(((var_G + 0.055) / 1.055), 2.4);
		else                   var_G = var_G / 12.92;
		if (var_B > 0.04045) var_B = pow(((var_B + 0.055) / 1.055), 2.4);
		else                   var_B = var_B / 12.92;

		var_R = var_R * 100.;
		var_G = var_G * 100.;
		var_B = var_B * 100.;

		//Observer. = 2°, Illuminant = D65
		float X = var_R * 0.4124 + var_G * 0.3576 + var_B * 0.1805;
		float Y = var_R * 0.2126 + var_G * 0.7152 + var_B * 0.0722;
		float Z = var_R * 0.0193 + var_G * 0.1192 + var_B * 0.9505;


		float var_X = X / 95.047;         //ref_X =  95.047   Observer= 2°, Illuminant= D65
		float var_Y = Y / 100.000;          //ref_Y = 100.000
		float var_Z = Z / 108.883;          //ref_Z = 108.883

		if (var_X > 0.008856) var_X = pow(var_X, (1. / 3.));
		else                    var_X = (7.787 * var_X) + (16. / 116.);
		if (var_Y > 0.008856) var_Y = pow(var_Y, (1. / 3.));
		else                    var_Y = (7.787 * var_Y) + (16. / 116.);
		if (var_Z > 0.008856) var_Z = pow(var_Z, (1. / 3.));
		else                    var_Z = (7.787 * var_Z) + (16. / 116.);

		float3 LAB = float3((116. * var_Y) - 16., 500. * (var_X - var_Y), 200. * (var_Y - var_Z));
		return LAB;

	}

	//http://www.easyrgb.com/index.php?X=MATH&H=01#text1
	float3 lab2rgb(float3 lab)
	{
		float var_Y = (lab.x + 16.) / 116.;
		float var_X = lab.y / 500. + var_Y;
		float var_Z = var_Y - lab.z / 200.;

		if (pow(var_Y, 3) > 0.008856) var_Y = pow(var_Y, 3);
		else                      var_Y = (var_Y - 16. / 116.) / 7.787;
		if (pow(var_X, 3) > 0.008856) var_X = pow(var_X, 3);
		else                      var_X = (var_X - 16. / 116.) / 7.787;
		if (pow(var_Z, 3) > 0.008856) var_Z = pow(var_Z, 3);
		else                      var_Z = (var_Z - 16. / 116.) / 7.787;

		float X = 95.047 * var_X;    //ref_X =  95.047     Observer= 2°, Illuminant= D65
		float Y = 100.000 * var_Y;   //ref_Y = 100.000
		float Z = 108.883 * var_Z;    //ref_Z = 108.883


		var_X = X / 100.;       //X from 0 to  95.047      (Observer = 2°, Illuminant = D65)
		var_Y = Y / 100.;       //Y from 0 to 100.000
		var_Z = Z / 100.;      //Z from 0 to 108.883

		float var_R = var_X * 3.2406 + var_Y * -1.5372 + var_Z * -0.4986;
		float var_G = var_X * -0.9689 + var_Y * 1.8758 + var_Z * 0.0415;
		float var_B = var_X * 0.0557 + var_Y * -0.2040 + var_Z * 1.0570;

		if (var_R > 0.0031308) var_R = 1.055 * pow(var_R, (1 / 2.4)) - 0.055;
		else                     var_R = 12.92 * var_R;
		if (var_G > 0.0031308) var_G = 1.055 * pow(var_G, (1 / 2.4)) - 0.055;
		else                     var_G = 12.92 * var_G;
		if (var_B > 0.0031308) var_B = 1.055 * pow(var_B, (1 / 2.4)) - 0.055;
		else                     var_B = 12.92 * var_B;

		float3 RGB = float3(var_R, var_G, var_B);
		return RGB;
	}

float3 YCrCb2rgb(float3 color){
        float r = color.r-.5 + 1.403 * (color.g);
        float g = color.r - .5 - 0.714 * (color.g ) - 0.344 * (color.b );
        float b = color.r - .5 + 1.773 * (color.b );
        return float3(r, g, b);
    }

    float3 rgb2YCrCb(float3 color) {
        float Y = 0.299*color.r + 0.587 * color.g + 0.114*color.b ;
        float Cr = (color.r - Y) * 0.713 ;
        float Cb = (color.b - Y) * 0.564 ;
        return float3(Y+.5, Cr, Cb);
    }