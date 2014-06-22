using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace RayTracer
{
    struct Normal3D
    {
        private Vector3D vec;
        public Normal3D(double x, double y, double z)
        {
            vec = new Vector3D(x, y, z);
            vec.Normalize();
        }
        public Normal3D(Vector3D vector)
        {
            vec = vector;
            vec.Normalize();
        }
        public static Normal3D operator +(Normal3D normal, Vector3D vector)
        {
            return new Normal3D(normal.vec + vector);
        }
        public static Normal3D operator -(Normal3D normal, Vector3D vector)
        {
            return new Normal3D(normal.vec - vector);
        }
        public static Normal3D operator +(Vector3D vector, Normal3D normal)
        {
            return new Normal3D(normal.vec + vector);
        }
        public static Normal3D operator -(Vector3D vector, Normal3D normal)
        {
            return new Normal3D(vector - normal.vec);
        }
        public static Normal3D operator +(Normal3D normal1, Normal3D normal2)
        {
            return new Normal3D(normal1.vec + normal2.vec);
        }
        public static Normal3D operator -(Normal3D normal1, Normal3D normal2)
        {
            return new Normal3D(normal1.vec - normal2.vec);
        }
        public static implicit operator Vector3D(Normal3D normal)
        {
            return normal.vec;
        }
        public static implicit operator Normal3D(Vector3D vector)
        {
            return new Normal3D(vector);
        }
    }

    class Ray
    {
        private Point3D pos;
        private Vector3D dir;
        private double t_min;
        private double t_max;
        public Ray(Point3D p1, Point3D p2)
        {
            this.pos = p1;
            this.dir = p2 - p1;
            t_max = this.dir.Length;
            t_min = 0.0001;
            this.dir.Normalize();
        }

        public Ray(Point3D p1, Point3D p2, double maxDistance)
            : this(p1, p2)
        {
            t_max = maxDistance;
        }

        public Ray(Vector3D start, Vector3D end) : this((Point3D)start, (Point3D)end) {}
        
        public Vector3D Dir
        {
            get { return dir; }
        }
        public Vector3D Start
        {
            get { return (Vector3D)pos; }
        }
        public Point3D StartPoint
        {
            get { return pos; }
        }
        public Point3D EndPoint
        {
            get { return pos + dir * t_max; }
        }
        public bool InRange(double parameter)
        {
            return (t_min < parameter) && (parameter < t_max);
        }
    }

    /// <summary>
    /// Store the local geometry at the intersection point. May need to store
    /// other quantities (eg. texture coordinate) in a more complicated raytracer.
    /// </summary>
    class LocalGeo
    {
        public Point3D Pos;
        public Normal3D Normal;
        public LocalGeo() { }
        public LocalGeo(Point3D pos, Normal3D normal)
        {
            this.Pos = pos;
            this.Normal = normal;
        }
    }

    /// <summary>
    /// Support Point, Vector, Normal, Ray, LocalGeo transformation by
    /// operator * overloading
    /// </summary>
    class Transformation
    {
        // Storing matrix m and its inverse transpose, minvt (for transforming normal)
        private Matrix3D m;
        private Matrix3D minvt;

        public static Ray operator *(Transformation trans, Ray ray)
        {
            Point3D startPos = trans.m.Transform(ray.StartPoint);
            Point3D endPos = trans.m.Transform(ray.EndPoint);
            return new Ray(startPos, endPos);
        }

        public static LocalGeo operator *(Transformation trans, LocalGeo local)
        {
            return new LocalGeo(trans.m.Transform(local.Pos), trans.minvt.Transform(local.Normal));
        }
    }
}
