using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace RayTracer
{
    class ScenePrototype
    {
        private int pixelWidth;
        private int pixelHeight;
        private double realWidth = 4;
        private double realHeight = 4;
        private Square square = new Square(new Point3D(0, 0, -1), new Vector3D(0, -1, 1), 2);
        private Triangle triangle = new Triangle(new Point3D(-1, 0, -1), new Point3D(0, 1, -1), new Point3D(1, 0, -1));

        public ScenePrototype(int pixelWidth, int pixelHeight)
        {
            this.pixelWidth = pixelWidth;
            this.pixelHeight = pixelHeight;
        }

        public bool Intersect(int x, int y)
        {
            double realX = realWidth * x / pixelWidth - realWidth / 2;
            //double realY = realHeight * (pixelHeight - y) / pixelHeight - realHeight / 2;
            double realY = realHeight / 2 - realHeight * y / pixelHeight; // y inverse
            //return square.Intersect(new Ray(new Point3D(realX, realY, 0), new Point3D(realX, realY, -10)));
            return square.IntersectP(new Ray(new Point3D(0, 0, 5), new Point3D(realX, realY, 0)));
            //return triangle.Intersect(new Ray(new Point3D(0, 0, 5), new Point3D(realX, realY, 0)));
        }
    }

    // Store screen coordinate
    class Sample
    {
        public double X, Y;
        public int I, J;
        public Sample(int i, int j, double x, double y)
        {
            this.I = i;
            this.J = j;
            this.X = x;
            this.Y = y;
        }
    }

    /// <summary>
    /// It will generate (x,y) of a screen sample and return true.
    /// Next time it gets called, it will generate another sample for the next pixel.
    /// It will return false when all the samples from all the pixels
    /// are generated. (In our case, we generate 1 sample per pixel, at the pixel sample.
    /// Later on, if we want to do multi-sample per pixel, we need to modify this class.
    /// </summary>
    class Sampler
    {
        private int pixelCountX;
        private int pixelCountY;
        private const double REAL_WIDTH = 4;
        private const double REAL_HEIGHT = 4;
        public Sampler(int pixelCountX, int pixelCountY)
        {
            this.pixelCountX = pixelCountX;
            this.pixelCountY = pixelCountY;
        }
        //public bool getSample(Sample* sample);
        public IEnumerable<Sample> GetSample()
        {
            for (int i = 0; i < pixelCountX; i++)
                for (int j = 0; j < pixelCountY; j++)
                {
                    double realX = REAL_WIDTH * i / pixelCountX - REAL_WIDTH / 2;
                    double realY = REAL_HEIGHT / 2 - REAL_HEIGHT * j / pixelCountY; // y inverse
                    yield return new Sample(i, j, realX, realY);
                }
        }
    }

    /// <summary>
    /// This is an abstract class that will generate a ray starting from
    /// the position stored in local to the position of the light source.
    /// You might want to consider creating 2 derived classes for
    /// point light source and directional light source.
    /// For directional light, the origin of the ray is the same, and the ray points to the light direction, however, t_max is infinity.
    /// </summary>
    class Light
    {
        private Point3D pos;
        private RTColor color;
        public Light(Point3D pos, RTColor color)
        {
            this.pos = pos;
            this.color = color;
        }
        //void generateLightRay(LocalGeo& local, Ray* lray, Color* lcolor);
        public Ray GenerateLightRay(LocalGeo local, out RTColor lcolor)
        {
            lcolor = new RTColor(color);
            return new Ray(local.Pos, this.pos);
        }
    }

    /// <summary>
    /// Create a ray starting from the camera that passes through the
    /// corresponding pixel (sample.x, sample.y) on the image plane.
    /// </summary>
    class Camera
    {
        private Point3D pos;
        private double maxDistance = 50;
        public Camera(Point3D pos)
        {
            this.pos = pos;
        }
        //void generateRay(Sample& sample, Ray* ray);
        public Ray GenerateRay(Sample sample)
        {
            return new Ray(pos, new Point3D(sample.X, sample.Y, 0), maxDistance);
        }
    }

    /// <summary>
    /// Beware when you generate reflection ray, make sure the ray don’t start
    /// exactly on the surface, or the intersection routine may return
    /// intersection point at the starting point of the ray. (This apply to light
    /// ray generation as well)
    /// </summary>
    class RTracer
    {        
        private AggregatePrimitive primitive;
        private List<Light> lights;

        public RTracer(AggregatePrimitive primitive, List<Light> lights)
        {
            this.primitive = primitive;
            this.lights = lights;
        }
        /*void trace(Ray& ray, int depth, Color* color) 
        {
            //if (depth exceed some threshold)
            {
                //Make the color black and return
            }
            if (!primitive.intersect(ray, &thit, &in)
            {
                // No intersection
                //Make the color black and return
            }
            // Obtain the brdf at intersection point
            in.primitive->getBRDF(in.local, &brdf);
            // There is an intersection, loop through all light source
            for (i = 0; i < #lights; i++)
            {
                lights[i].generateLightRay(in.local, &lray, &lcolor);
                // Check if the light is blocked or not
                if (!primitive->intersectP(lray))
                    // If not, do shading calculation for this
                    // light source
                    *color += shading(in.local, brdf, lray, lcolor);
            }
            // Handle mirror reflection
            if (brdf.kr > 0)
            {
                reflectRay = createReflectRay(in.local, ray);
                // Make a recursive call to trace the reflected ray
                trace(reflectRay, depth+1, &tempColor);
                *color += brdf.kr * tempColor;
            }
        }*/

        public RTColor Trace(Ray ray, int depth)
        {
            //if (depth exceed some threshold)
            if (depth > 2)
            {
                //Make the color black and return
                return new RTColor(); 
            }
            double thit = double.MaxValue;
            Intersection intersec = new Intersection();
            if (!primitive.Intersect(ray, ref thit, intersec))
            {
                // No intersection
                //Make the color black and return
                //return Color.Black;
                return new RTColor(); // black color
            }
            // Obtain the brdf at intersection point
            BRDF brdf = intersec.Primitive.GetBRDF(intersec.Local);
            // There is an intersection, loop through all light source
            RTColor color = new RTColor();
            for (int i = 0; i < lights.Count; i++)
            {
                RTColor lcolor;
                Ray lray = lights[i].GenerateLightRay(intersec.Local, out lcolor);
                // Check if the light is blocked or not
                if (!primitive.IntersectP(lray))
                {
                    // If not, do shading calculation for this
                    // light source                    
                    color += Shading(ray.StartPoint, intersec.Local, brdf, lray, lcolor);
                }
            }
            // Handle mirror reflection
            if (brdf.kr > 0)
            {
                Ray reflectRay = CreateReflectRay(intersec.Local, ray);
                // Make a recursive call to trace the reflected ray
                RTColor tempColor = Trace(reflectRay, depth + 1);
                color += tempColor * brdf.kr;
            }
            return color;
        }

        private Ray CreateReflectRay(LocalGeo local, Ray ray)
        {
            Vector3D straightVec = ray.EndPoint - ray.StartPoint;
            Vector3D reflectVec = straightVec - 2 * Vector3D.DotProduct(straightVec, local.Normal) * (Vector3D)local.Normal;
            Ray reflectRay = new Ray(local.Pos, local.Pos + reflectVec);
            return reflectRay;
        }

        private RTColor ComputeLight(Vector3D direction, RTColor lightcolor, Normal3D normal,
            Normal3D halfVec, RTColor diffuse, RTColor specular, double shininess)
        {
            double nDotL = Vector3D.DotProduct(normal, direction);            
            RTColor lambert = diffuse * lightcolor * Math.Max(nDotL, 0.0);

            double nDotH = Vector3D.DotProduct(normal, halfVec);
            RTColor phong = specular * lightcolor * Math.Pow(Math.Max(nDotH, 0.0), shininess);

            RTColor retVal = lambert + phong;
            
            return retVal;
        }

        private RTColor Shading(Point3D camPos, LocalGeo local, BRDF brdf, Ray lray, RTColor lcolor)
        {
            /*const vec3 eyepos = vec3(0, 0, 0);
            vec4 _mypos = gl_ModelViewMatrix * myvertex;
            vec3 mypos = _mypos.xyz / _mypos.w; // Dehomogenize current location 
            vec3 eyedirn = normalize(eyepos - mypos);

            // Compute normal, needed for shading. 
            // Simpler is vec3 normal = normalize(gl_NormalMatrix * mynormal) ; 
            vec3 _normal = (gl_ModelViewMatrixInverseTranspose * vec4(mynormal, 0.0)).xyz;
            vec3 normal = normalize(_normal);

            finalcolor = ambient + emission;
            vec4 lightposn = lightposn[i];
            vec4 lightcolor = lightcolor[i];

            vec3 position = lightposn.xyz / lightposn.w;
            vec3 direction = normalize(position - mypos);
            vec3 half_ = normalize (direction + eyedirn) ;*/
            Normal3D direction = lray.Dir;
            Normal3D eyedirn = camPos - local.Pos;
            Normal3D halfVec = direction + eyedirn;

            double shininess = 100;
            RTColor finalcolor = brdf.ka + ComputeLight(direction, lcolor, local.Normal, halfVec, brdf.kd, brdf.ks, shininess);
            // for debugging
            //if (Double.IsNaN(finalcolor.R))
            //    throw new Exception("color is nan");
            return finalcolor;
        }
    }

    /// <summary>
    /// Can be implemented just by a 2D array of Color (Later on, we can
    /// implement more complicated things such as multi-sample per pixel, or
    /// post processing, eg. tone mapping in this class)
    /// </summary>
    class Film
    {
        private BitmapImage image;
        private int pixelSizeX;
        private int pixelSizeY;
        private RTColor[,] colors;
        public Film(BitmapImage image, int pixCountX, int pixCountY)
        {
            this.image = image;
            this.pixelSizeX = pixCountX;
            this.pixelSizeY = pixCountY;
            //colors = new System.Drawing.Color[pixelSizeX, pixelSizeY];
            colors = new RTColor[pixelSizeX, pixelSizeY];
        }
        // Will write the color to (sample.x, sample.y) on the image
        //void commit(Sample& sample, Color& color)
        public void Commit(Sample sample, RTColor color)
        {
            colors[sample.I, sample.J] = color;
        }
        // Output image to a file
        public void WriteImage()
        {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(pixelSizeX, pixelSizeY);
            for (int i = 0; i < pixelSizeX; i++)
                for (int j = 0; j < pixelSizeY; j++)
                {
                    int r = double.IsNaN(colors[i, j].R) ? 0 : (int)(255 * colors[i, j].R);
                    int g = double.IsNaN(colors[i, j].G) ? 0 : (int)(255 * colors[i, j].G);
                    int b = double.IsNaN(colors[i, j].B) ? 0 : (int)(255 * colors[i, j].B);
                    Color color = Color.FromArgb(r, g, b);
                    bmp.SetPixel(i, j, color);
                }
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
        }
    }

    class Scene
    {
        private const int BMP_SIZE_X = 250;
        private const int BMP_SIZE_Y = 250;

        Light light1 = new Light(new Point3D(0, 5, 5), new RTColor(1, 1, 1));
        //Light light2 = new Light(new Point3D(5, 10, 5), new RTColor(0.9, 0.6, 0.6));
        Light light2 = new Light(new Point3D(5, 10, 5), new RTColor(0.6, 0.6, 0.6));
        //Light light3 = new Light(new Point3D(-5, 0, 5), new RTColor(0.5, 0.5, 1));
        Light light3 = new Light(new Point3D(-5, 0, 5), new RTColor(1, 1, 1));

        // Sphere #1
        Sphere sphere1 = new Sphere(new Point3D(-.8, -.6, 0), .4);
        // Sphere #2
        Sphere sphere2 = new Sphere(new Point3D(.8, -.6, 0), .4);
        // Sphere #2
        Sphere sphere3 = new Sphere(new Point3D(0, -.6, .8), .4);
        // BigSphere
        Sphere bigSphere = new Sphere(new Point3D(0, .2, 0), .8);
        // Square #1
        private Square square1 = new Square(new Point3D(0, -1, -1), new Vector3D(0, 1, 0), 2);
        //private Triangle triangle = new Triangle(new Point3D(-1, 0, -1), new Point3D(0, 1, -1), new Point3D(1, 0, -1));

        // kd, ks, ka are diffuse, specular and ambient component respectively kr is the mirror reflection coefficient 
        //RTColor kd1 = new RTColor(0.6, 0, 0), ks1 = new RTColor(0.7, 0.7, 0.7), ka1 = new RTColor(0.1, 0, 0), kr1 = new RTColor();        
        //RTColor kd1 = new RTColor(0.25, 0.25, 1), ks1 = new RTColor(1, 1, 1), ka1 = new RTColor(0, 0, 0), kr1 = new RTColor();
        RTColor ks1 = new RTColor(1, 1, 1), ka1 = new RTColor(0, 0, 0);
        RTColor kd1 = new RTColor(1, 1, 0.25);
        RTColor kd2 = new RTColor(.25, 1, 0.25);
        RTColor kd3 = new RTColor(1, .25, 0.25);
        RTColor bigSphereKd = new RTColor(.25, .25, 1);
        double kr1 = 1;
        RTColor floorWhiteKd = new RTColor(.9, .9, .9);
        RTColor floorBlackKd = new RTColor(.1, .1, .1);
        int squareNumberZ = 10;
        int squareNumberX = 16;
        int blackSquareNumber = 4;
        RTColor kubusKd = new RTColor(.5, .5, .5);

        private Sampler sampler;
        private Camera camera;
        private Film film;
        private RTracer raytracer;


        public Scene(BitmapImage image)
        {
            sampler = new Sampler(BMP_SIZE_X, BMP_SIZE_Y);
            film = new Film(image, BMP_SIZE_X, BMP_SIZE_Y);
            camera = new Camera(new Point3D(0, 0, 5));

            List<Light> lights = new List<Light>() { light2, light3 };
                        
            GeometricPrimitive spherePrimitive1 = new GeometricPrimitive(sphere1, new Material(new BRDF(kd1, ks1, ka1, kr1)));
            GeometricPrimitive spherePrimitive2 = new GeometricPrimitive(sphere2, new Material(new BRDF(kd2, ks1, ka1, kr1)));
            GeometricPrimitive spherePrimitive3 = new GeometricPrimitive(sphere3, new Material(new BRDF(kd3, ks1, ka1, kr1)));
            GeometricPrimitive bigSpherePrimitive = new GeometricPrimitive(bigSphere, new Material(new BRDF(bigSphereKd, ks1, ka1, kr1)));
            //GeometricPrimitive squarePrimitive1 = new GeometricPrimitive(square1, new Material(new BRDF(floorWhiteKd1, ks1, ka1, kr1)));
            List<Primitive> primList = new List<Primitive>();
            primList.Add(spherePrimitive1);
            primList.Add(spherePrimitive2);
            primList.Add(spherePrimitive3);
            primList.Add(bigSpherePrimitive);
            // white squares
            for (int i = 0; i < squareNumberZ; i++)
            {
                for (int j = 0; j <= squareNumberX; j++)
                {
                    Square whiteSquare = new Square(new Point3D(-squareNumberX / 2 + j + 1, -1, 2 - j % 2 - i * 2), new Vector3D(0, 1, 0), 1);
                    primList.Add(new GeometricPrimitive(whiteSquare, new Material(new BRDF(floorWhiteKd, ks1, ka1, kr1))));
                }
            }
            // black squares
            for (int i = 0; i < blackSquareNumber; i++)
            {
                for (int j = 0; j <= blackSquareNumber; j++)
                {
                    Square blackSquare = new Square(new Point3D(-blackSquareNumber / 2 + j, -1, 2 - j % 2 - i * 2), new Vector3D(0, 1, 0), 1);
                    primList.Add(new GeometricPrimitive(blackSquare, new Material(new BRDF(floorBlackKd, ks1, ka1, kr1))));
                }
            }
            // kubus
            //Square face1 = new Square(new Point3D(-.53, -.25, -2), new Vector3D(-1, 0, 1), 1.5);
            //Square face1 = new Square(new Point3D(-.53, -.25, -.7), new Vector3D(-1, 0, 1), 1.5);
            //Square face2 = new Square(new Point3D(.53, -.25, -2), new Vector3D(1, 0, 1), 1.5);
            //primList.Add(new GeometricPrimitive(face1, new Material(new BRDF(kubusKd, ks1, ka1, kr1))));
            //primList.Add(new GeometricPrimitive(face2, new Material(new BRDF(kubusKd, ks1, ka1, kr1))));
            AggregatePrimitive primitives = new AggregatePrimitive(primList);

            raytracer = new RTracer(primitives, lights);
        }

        // This is the main rendering loop
        public void Render()
        {
            foreach (Sample sample in sampler.GetSample())
            {
                Ray ray = camera.GenerateRay(sample);
                RTColor color = raytracer.Trace(ray, 0);
                film.Commit(sample, color);
            }
            film.WriteImage();
        }
    }
}
