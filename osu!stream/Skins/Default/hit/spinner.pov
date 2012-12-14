// +w2048 +h2048 +ua +a0.01 +am2 +r4 +fn

#version 3.7

global_settings
{
    assumed_gamma 1
}

background
{
    color rgbt 1
}

camera
{
    orthographic
    location 1000*z
    look_at 1*z
    up 2*y
    right 2*x
}

// the main light is directly above so the spinner can be rotated ingame
// withint influencing its position and ruining its believability as a spinning object
light_source
{
    100*<0, 0, 1>
    color rgb 1
    parallel
    point_at 0
}

// the spinner is smooth and raised close to the edges, then it forms a bowl shape towards the center.
lathe
{
    cubic_spline
    5,
    <0.00, 0.60>
    <0.00, 0.80>
    <0.60, 1.00>
    <1.00, 1.00>
    <1.05, 0.80>
    rotate 90*x

    texture
    {
        pigment
        {
            function {cos(atan2(y, x)*4+8*pow(x*x+y*y, 0.25))*0.25}
            color_map
            {
                [0, 0.5 color rgb <0.005, 0.205, 1.000> color rgb <0.005, 0.205, 1.000>]
                [0.5, 1 color rgb <0.005, 0.005, 0.005> color rgb <0.005, 0.005, 0.005>]
            }
        }
        finish
        {
            diffuse 0.80 ambient 0.05 brilliance 20
            specular 0.01 roughness 0.01
        }
    }
}

