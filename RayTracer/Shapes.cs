using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace RayTracer
{
    /// <summary>
    /// Triangle and Sphere are probably best implemented here
    /// The intersection with the ray at t outside the range [t_min, t_max]
    /// should return false.
    /// </summary>
    abstract class Shape
    {
        // Test if ray intersects with the shape or not (in object space), if so,
        // return intersection point and normal
        //bool intersect(Ray& ray, float* thit, LocalGeo* local)
        virtual public bool Intersect(Ray ray, ref double thit, LocalGeo local)
        {
            local.Pos = new Point3D(0, 0, 0);
            local.Normal = new Normal3D(0, 0, 1);
            return false;
        }
                       
        // Same as intersect, but just return whether there is any intersection or not
        //abstract public bool IntersectP(Ray ray);
        virtual public bool IntersectP(Ray ray)
        {
            double thit = 0;
            LocalGeo local = new LocalGeo();
            return Intersect(ray, ref thit, local);
        }
    }
    
    class Square : Shape
    {
        private Point3D center;
        private Vector3D normal;
        private double size;

        public Square(Point3D center, Vector3D normal, double size)
        {
            this.center = center;
            this.normal = normal;
            this.normal.Normalize();
            this.size = size;
        }

        public override bool Intersect(Ray ray, ref double thit, LocalGeo local)
        {            
            double dividend = Vector3D.DotProduct(center - ray.StartPoint, normal);
            double divisor = Vector3D.DotProduct(ray.Dir, normal);
            double parameter = dividend / divisor;
            if (!ray.InRange(parameter))
                return false;
            Point3D intersectPoint = ray.StartPoint + ray.Dir * parameter;
            //Vector3D planeVec = ray.Start + ray.Dir * parameter - (Vector3D)center;
            Vector3D planeVec = intersectPoint - center;

            //local = new LocalGeo(intersectPoint, normal);
            local.Pos = intersectPoint;
            local.Normal = normal;
            thit = (ray.Dir * parameter).Length;

            Vector3D planeDirX = Vector3D.CrossProduct(new Vector3D(0, 1, 0), normal);
            if (planeDirX.Length == 0)
                planeDirX = new Vector3D(1, 0, 0);
            planeDirX.Normalize();
            Vector3D planeDirY = Vector3D.CrossProduct(normal, planeDirX);
            planeDirY.Normalize();
            double planePrX = Vector3D.DotProduct(planeVec, planeDirX);
            double planePrY = Vector3D.DotProduct(planeVec, planeDirY);
            return (planePrX <= size / 2) && (planePrX >= -size / 2) && (planePrY <= size / 2) && (planePrY >= -size / 2);
        }

        public override bool IntersectP(Ray ray)
        {
            double dividend = Vector3D.DotProduct(center - ray.StartPoint, normal);
            double divisor = Vector3D.DotProduct(ray.Dir, normal);
            double parameter = dividend / divisor;
            if (!ray.InRange(parameter))
                return false;
            Point3D intersectPoint = ray.StartPoint + ray.Dir * parameter;
            //Vector3D planeVec = ray.Start + ray.Dir * parameter - (Vector3D)center;
            Vector3D planeVec = intersectPoint - center;

            Vector3D planeDirX = Vector3D.CrossProduct(new Vector3D(0, 1, 0), normal);
            if (planeDirX.Length == 0)
                planeDirX = new Vector3D(1, 0, 0);
            planeDirX.Normalize();
            Vector3D planeDirY = Vector3D.CrossProduct(normal, planeDirX);
            planeDirY.Normalize();
            double planePrX = Vector3D.DotProduct(planeVec, planeDirX);
            double planePrY = Vector3D.DotProduct(planeVec, planeDirY);
            return (planePrX <= size / 2) && (planePrX >= -size / 2) && (planePrY <= size / 2) && (planePrY >= -size / 2);
        }
    }

    class Triangle : Shape
    {
        private Point3D pt1, pt2, pt3;
        public Triangle(Point3D pt1, Point3D pt2, Point3D pt3)
        {
            this.pt1 = pt1;
            this.pt2 = pt2;
            this.pt3 = pt3;
        }

        //public override bool IntersectP(Ray ray)
        public override bool Intersect(Ray ray, ref double thit, LocalGeo local)
        {
            Vector3D normal = Vector3D.CrossProduct((pt3 - pt1), (pt2 - pt1));
            normal.Normalize();

            double dividend = Vector3D.DotProduct(pt1 - ray.StartPoint, normal);
            double divisor = Vector3D.DotProduct(ray.Dir, normal);
            double parameter = dividend / divisor;

            Point3D intersectPoint = ray.StartPoint + ray.Dir * parameter;
            
            //local = new LocalGeo(intersectPoint, normal);
            local.Pos = intersectPoint;
            local.Normal = normal;

            //Vector3D P_3D = ray.Start + ray.Dir * parameter - (Vector3D)pt1;
            Vector3D P_3D = intersectPoint - pt1;
            Vector3D lateralVec1_3D = pt2 - pt1;
            Vector3D lateralVec2_3D = pt3 - pt1;

            Vector3D planeDirX = Vector3D.CrossProduct(new Vector3D(0, 1, 0), normal);
            planeDirX.Normalize();
            Vector3D planeDirY = Vector3D.CrossProduct(normal, planeDirX);
            planeDirY.Normalize();
            double Px = Vector3D.DotProduct(P_3D, planeDirX);
            double Py = Vector3D.DotProduct(P_3D, planeDirY);
            double ax = Vector3D.DotProduct(lateralVec1_3D, planeDirX);
            double ay = Vector3D.DotProduct(lateralVec1_3D, planeDirY);
            double bx = Vector3D.DotProduct(lateralVec2_3D, planeDirX);
            double by = Vector3D.DotProduct(lateralVec2_3D, planeDirY);

            //Px = beta * ax + gamma * bx;
            //Py = beta * ay + gamma * by;
            
            double gamma, beta;
            double det = by * ax - bx * ay;
            if (Math.Abs(det) > 0.0001)
            {
                gamma = (Py * ax - Px * ay) / det;
                if (ax != 0)
                    beta = (Px - gamma * bx) / ax;
                else
                    beta = (Py - gamma * by) / ay;
            }
            else
            {
                gamma = 0.5;
                if (ax != 0)
                    beta = (Px - gamma * bx) / ax;
                else
                    beta = (Py - gamma * by) / ay;
            }

            return (beta >= 0) && (beta <= 1) && (gamma >= 0) && (gamma <= 1) && (beta + gamma <= 1);
        }
    }

    class Sphere : Shape
    {
        private Point3D center;
        private double radius;

        public Sphere(Point3D center, double radius)
        {
            this.center = center;
            this.radius = radius;
        }

        public override bool Intersect(Ray ray, ref double thit, LocalGeo local)
        {            
            Vector3D P1 = ray.Dir;
            Vector3D P0C = ray.StartPoint - center;
            double a = Vector3D.DotProduct(P1, P1);
            double b = 2 * Vector3D.DotProduct(P1, P0C);
            double c = Vector3D.DotProduct(P0C, P0C) - radius * radius;
            double discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
                return false;
            double t1 = (-b + Math.Sqrt(discriminant)) / 2 / a;
            double t2 = (-b - Math.Sqrt(discriminant)) / 2 / a;
            double parameter = Math.Min(t1, t2);
            if (!ray.InRange(parameter))
                return false;
            Point3D intersectPoint = ray.StartPoint + ray.Dir * parameter;
            //Normal3D normal = intersectPoint - center;
            //local = new LocalGeo(intersectPoint, normal);
            local.Pos = intersectPoint;
            local.Normal = intersectPoint - center;
            thit = (ray.Dir * parameter).Length;
            return true;
        }

        public override bool IntersectP(Ray ray)
        {
            Vector3D P1 = ray.Dir;
            Vector3D P0C = ray.StartPoint - center;
            double a = Vector3D.DotProduct(P1, P1);
            double b = 2 * Vector3D.DotProduct(P1, P0C);
            double c = Vector3D.DotProduct(P0C, P0C) - radius * radius;
            double discriminant = b * b - 4 * a * c;
            if (discriminant < 0) 
                return false;
            else
            {
                double t1 = (-b + Math.Sqrt(discriminant)) / 2 / a;
                double t2 = (-b - Math.Sqrt(discriminant)) / 2 / a;
                double parameter = Math.Min(t1, t2);
                return ray.InRange(parameter);
            }
        }
    }
}
