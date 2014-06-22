using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracer
{
    /// <summary>
    /// Abstract class for primitives in the scene
    /// </summary>
    abstract class Primitive
    {
        abstract public bool Intersect(Ray ray, ref double thit, Intersection intersec);   
        abstract public bool IntersectP(Ray ray);
        abstract public BRDF GetBRDF(LocalGeo local);
    }

    class Intersection
    {
        public LocalGeo Local;
        public Primitive Primitive;
    }

    /// <summary>
    /// Support +,- with other color
    /// Support scalar *, /
    /// May support conversion from xyz
    /// </summary>
    struct RTColor
    {
        private double r;
        private double g;
        private double b;
        public RTColor(double r, double g, double b)
        {
            this.r = r > 1 ? 1 : r;
            this.g = g > 1 ? 1 : g;
            this.b = b > 1 ? 1 : b;
        }

        public RTColor(RTColor color)
        {
            this.r = color.r;
            this.g = color.g;
            this.b = color.b;
        }

        public static RTColor operator +(RTColor color1, RTColor color2)
        {
            return new RTColor(color1.r + color2.r, color1.g + color2.g, color1.b + color2.b);
        }

        public static RTColor operator -(RTColor color1, RTColor color2)
        {
            return new RTColor(color1.r - color2.r, color1.g - color2.g, color1.b - color2.b);
        }

        public static RTColor operator *(RTColor color1, RTColor color2)
        {
            return new RTColor(color1.r * color2.r, color1.g * color2.g, color1.b * color2.b);
        }

        public static RTColor operator *(RTColor color, double scalar)
        {
            return new RTColor(color.r * scalar, color.g * scalar, color.b * scalar);
        }

        public static RTColor operator /(RTColor color, double scalar)
        {
            return new RTColor(color.r / scalar, color.g / scalar, color.b / scalar);
        }

        public double R { get { return r; } }
        public double G { get { return g; } }
        public double B { get { return b; } }
    }

    /// <summary>
    /// Storing information enough for shading (it is not the actual BRDF function
    /// in the rendering equation that will be covered later in the semester)
    /// </summary>
    struct BRDF
    {
        // kd, ks, ka are diffuse, specular and ambient component respectively
        // kr is the mirror reflection coefficient
        public RTColor kd;
        public RTColor ks;
        public RTColor ka;
        //public RTColor kr;
        public double kr;

        //public BRDF(RTColor kd, RTColor ks, RTColor ka, RTColor kr)
        public BRDF(RTColor kd, RTColor ks, RTColor ka, double kr)
        {
            this.kd = kd; this.ks = ks; this.ka = ka; this.kr = kr;
        }
    }

    /// <summary>
    /// Class for storing material. For this example, it just returns a constant
    /// material regardless of what local is. Later on, when we want to support
    /// texture mapping, this need to be modified.
    /// </summary>
    class Material
    {
        private BRDF constantBRDF;

        public Material(BRDF brdf) { constantBRDF = brdf; }
        //void getBRDF(LocalGeo local, BRDF brdf)
        public BRDF GetBRDF(LocalGeo local)
        {
            return constantBRDF;
        }
    }

    class GeometricPrimitive : Primitive
    {
        private Transformation objToWorld;
        private Transformation worldToObj;
        private Shape shape;
        private Material material;

        public GeometricPrimitive(Shape shape, Material material)
        {
            this.shape = shape;
            this.objToWorld = new Transformation();
            this.worldToObj = new Transformation();
            this.material = material;
        }
        //override bool Intersect(Ray& ray, float* thit, Intersection in)
        public override bool Intersect(Ray ray, ref double thit, Intersection intersec)
        {
            Ray oray = worldToObj * ray;
            LocalGeo olocal = new LocalGeo();                                 
            if (!shape.Intersect(oray, ref thit, olocal))
                return false;
            intersec.Primitive = this;
            intersec.Local = objToWorld * olocal;
            return true;
        }

        public override bool IntersectP(Ray ray)
        {
            Ray oray = worldToObj * ray;
            return shape.IntersectP(oray);                        
        }
 
        //void getBRDF(LocalGeo& local, BRDF* brdf)
        public override BRDF GetBRDF(LocalGeo local)
        {
            return material.GetBRDF(local);
        }
    }

    /// <summary>
    /// Constructor store the STL vector of pointers to primitives.
    /// Intersect just loops through all the primitives in the list and
    /// call the intersect routine. Compare thit and return that of the nearest one (because we want the first hit).
    /// Also, the in->primitive should be set to the pointer to that primitive.
    /// When you implement acceleration structure, it will replace this class.
    /// </summary>
    class AggregatePrimitive : Primitive
    {
        private List<Primitive> primitives;
        public AggregatePrimitive(List<Primitive> list)
        {
            primitives = list;
        }

        // ??????
        public override bool Intersect(Ray ray, ref double thit, Intersection intersec)
        {
            double minLength = thit;
            double currThit = thit;
            Intersection currIntersec = new Intersection();
            foreach (Primitive prim in primitives)
                if (prim.Intersect(ray, ref currThit, currIntersec))
                {
                    if (currThit < minLength)
                    {
                        minLength = currThit;
                        intersec.Local = currIntersec.Local;
                        intersec.Primitive = currIntersec.Primitive;
                    }
                }
            //if (intersec.Primitive != null)
            if (minLength < thit)
            {
                thit = minLength;
                return true;
            }
            else
                return false;
        }

        public override bool IntersectP(Ray ray)
        {
            foreach (Primitive prim in primitives)
                if (prim.IntersectP(ray))
                    return true;
            return false;
        }

        public override BRDF GetBRDF(LocalGeo local)
        {
            //exit(1);
            throw new Exception("AggregatePrimitive.GetBRDF not implemented");
            // This should never get called, because in->primitive will
            // never be an aggregate primitive
        }
    }
}
