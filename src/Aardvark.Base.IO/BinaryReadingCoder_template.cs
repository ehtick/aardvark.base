using Aardvark.Base;
using System;
using System.Collections.Generic;

namespace Aardvark.Base.Coder
{
    // AUTO GENERATED CODE - DO NOT CHANGE!

    //# var directlyCodeableTypes = Meta.DirectlyCodeableTypes.Map(t =>t.Name);
    //# var geometryTypes = Meta.GeometryTypes.Map(t => t.Name);
    //# var specialSimpleTypes = new[] { "bool", "char", "string", "Type", "Guid", "Symbol" };
    //# var structTypes = new List<string>();
    //# var fastStructTypes = new List<string>();
    public partial class BinaryReadingCoder
    {
        #region Vectors

        //# foreach (var t in Meta.VecTypes) { fastStructTypes.Add(t.Name);
        //# var name = Meta.GetXmlTypeName(t.Name);
        public void Code__name__(ref __t.Name__ value) 
        {
            //# t.Fields.ForEach(f => {
            value.__f__ = m_reader.__t.FieldType.Read__();
            //# });
        }

        //# }
        #endregion

        #region Matrices

        //# foreach (var t in Meta.MatTypes) { fastStructTypes.Add(t.Name);
        //# var name = Meta.GetXmlTypeName(t.Name);
        public void Code__name__(ref __t.Name__ value) { value = m_reader.__t.Read__(); }
        //# }

        #endregion

        #region Ranges and Boxes

        //# foreach (var t in Meta.RangeAndBoxTypes) { fastStructTypes.Add(t.Name);
        //# var name = Meta.GetXmlTypeName(t.Name);
        public void Code__name__(ref __t.Name__ value)
        {
            value.Min = m_reader.__t.LimitType.Read__();
            value.Max = m_reader.__t.LimitType.Read__();
        }

        //# }
        #endregion

        #region Geometry Types

        //# foreach (var isDouble in new[] { false, true }) {
        //#   var ftype = isDouble ? "Double" : "Float";
        //#   var tc = isDouble ? "d" : "f";
        public void CodeCircle2__tc__(ref Circle2__tc__ v)
        {
            CodeV2__tc__(ref v.Center); Code__ftype__(ref v.Radius);
        }

        public void CodeLine2__tc__(ref Line2__tc__ v)
        {
            CodeV2__tc__(ref v.P0); CodeV2__tc__(ref v.P1);
        }

        public void CodeLine3__tc__(ref Line3__tc__ v)
        {
            CodeV3__tc__(ref v.P0); CodeV3__tc__(ref v.P1);
        }

        public void CodePlane2__tc__(ref Plane2__tc__ v)
        {
            CodeV2__tc__(ref v.Normal); Code__ftype__(ref v.Distance);
        }

        public void CodePlane3__tc__(ref Plane3__tc__ v)
        {
            CodeV3__tc__(ref v.Normal); Code__ftype__(ref v.Distance);
        }

        public void CodePlaneWithPoint3__tc__(ref PlaneWithPoint3__tc__ v)
        {
            CodeV3__tc__(ref v.Normal); CodeV3__tc__(ref v.Point);
        }

        public void CodeQuad2__tc__(ref Quad2__tc__ v)
        {
            CodeV2__tc__(ref v.P0); CodeV2__tc__(ref v.P1); CodeV2__tc__(ref v.P2); CodeV2__tc__(ref v.P3);
        }

        public void CodeQuad3__tc__(ref Quad3__tc__ v)
        {
            CodeV3__tc__(ref v.P0); CodeV3__tc__(ref v.P1); CodeV3__tc__(ref v.P2); CodeV3__tc__(ref v.P3);
        }

        public void CodeRay2__tc__(ref Ray2__tc__ v)
        {
            CodeV2__tc__(ref v.Origin); CodeV2__tc__(ref v.Direction);
        }

        public void CodeRay3__tc__(ref Ray3__tc__ v)
        {
            CodeV3__tc__(ref v.Origin); CodeV3__tc__(ref v.Direction);
        }

        public void CodeSphere3__tc__(ref Sphere3__tc__ v)
        {
            CodeV3__tc__(ref v.Center); Code__ftype__(ref v.Radius);
        }

        public void CodeTriangle2__tc__(ref Triangle2__tc__ v)
        {
            CodeV2__tc__(ref v.P0); CodeV2__tc__(ref v.P1); CodeV2__tc__(ref v.P2);
        }

        public void CodeTriangle3__tc__(ref Triangle3__tc__ v)
        {
            CodeV3__tc__(ref v.P0); CodeV3__tc__(ref v.P1); CodeV3__tc__(ref v.P2);
        }

        //# }
        #endregion

        #region Colors

        //# foreach (var t in Meta.ColorTypes) { fastStructTypes.Add(t.Name);
        //# var name = Meta.GetXmlTypeName(t.Name);
        public void Code__name__(ref __t.Name__ value)
        {
            //# t.Fields.ForEach(f => {
            value.__f__ = m_reader.__t.FieldType.Read__();
            //# });
        }

        //# }
        #endregion

        #region Trafos

        //# foreach (var t in Meta.TrafoTypes) { structTypes.Add(t.Name);
        //# var name = Meta.GetXmlTypeName(t.Name);
        public void Code__name__(ref __t.Name__ value) { value = m_reader.__t.Read__(); }
        //# }

        #endregion

        #region Tensors

        //# foreach (var t in geometryTypes) structTypes.Add(t);
        //# var simpleTypes = directlyCodeableTypes.Concat(specialSimpleTypes).Concat(geometryTypes);
        //# var genericTensorTypes = new[] { "Vector", "Matrix", "Volume", "Tensor" };
        //# var genericTensorSizes = new[] { "long", "V2l", "V3l", "long[]" };
        //# genericTensorTypes.ForEach((tt, ti) => {
        //#     var ts = genericTensorSizes[ti]; var tsn = Meta.GetXmlTypeName(ts);
        //#     simpleTypes.ForEach((t, i) => {
        //#         var type = tt + "<" + t + ">";
        //#         structTypes.Add(type);
        //# var dname = Meta.GetXmlTypeName(t + "[]");
        //# var name = Meta.GetXmlTypeName(type);
        public void Code__name__(ref __type__ value)
        {
            __t__[] data = null; Code__dname__(ref data);
            long origin = 0L; CodeLong(ref origin);
            __ts__ size = default(__ts__); Code__tsn__(ref size);
            __ts__ delta = default(__ts__); Code__tsn__(ref delta);
            value = new __type__(data, origin, size, delta);
        }

        //#     });
        //# });
        #endregion

        #region Arrays

        //# fastStructTypes.ForEach(t => {
        //# var name = Meta.GetXmlTypeName(t + "[]");
        public void Code__name__(ref __t__[] value)
        {
            long count = CodeCountLong(ref value);
            if (count < 1) return;
            m_reader.ReadArray(value, 0, count);
        }

        //# });
        //# structTypes.ForEach(t => {
        //# var ename = Meta.GetXmlTypeName(t);
        //# var name = Meta.GetXmlTypeName(t + "[]");
        public void Code__name__(ref __t__[] value)
        {
            long count = CodeCountLong(ref value);
            if (count < 1) return;
            for (long i = 0; i < count; i++) Code__ename__(ref value[i]);
        }

        //# });
        #endregion

        #region Multi-Dimensional Arrays

        //# directlyCodeableTypes.ForEach(t => {
        //# var name2d = Meta.GetXmlTypeName(t + "[,]");
        public void Code__name2d__(ref __t__[,] value)
        {
            long[] countArray = CodeCountLong(ref value);
            long c0 = countArray[0]; if (c0 < 1) return;
            long c1 = countArray[1]; if (c1 < 1) return;
            m_reader.ReadArray(value, c0 * c1);
        }

        //# var name3d = Meta.GetXmlTypeName(t + "[,,]");
        public void Code__name3d__(ref __t__[, ,] value)
        {
            long[] countArray = CodeCountLong(ref value);
            long c0 = countArray[0]; if (c0 < 1) return;
            long c1 = countArray[1]; if (c1 < 1) return;
            long c2 = countArray[2]; if (c2 < 1) return;
            m_reader.ReadArray(value, c0 * c1 * c2);
        }

        //# });
        #endregion

        #region Jagged Multi-Dimensional Arrays

        //# directlyCodeableTypes.ForEach(t => {
        //# var ename2d = Meta.GetXmlTypeName(t + "[]");
        //# var name2d = Meta.GetXmlTypeName(t + "[][]");
        public void Code__name2d__(ref __t__[][] value)
        {
            long count = CodeCountLong(ref value);
            if (count < 1) return;
            for (long i = 0; i < count; i++) Code__ename2d__(ref value[i]);
        }

        //# var ename3d = Meta.GetXmlTypeName(t + "[][]");
        //# var name3d = Meta.GetXmlTypeName(t + "[][][]");
        public void Code__name3d__(ref __t__[][][] value)
        {
            long count = CodeCountLong(ref value);
            if (count < 1) return;
            for (long i = 0; i < count; i++) Code__ename3d__(ref value[i]);
        }

        //# });
        #endregion

        #region Lists

        //# fastStructTypes.ForEach(t => {
        //# var name = Meta.GetXmlTypeName("List<" + t + ">");
        public void Code__name__(ref List<__t__> value)
        {
            int count = CodeCount(ref value);
            if (count < 1) return;
            m_reader.ReadList(value, 0, count);
        }

        //# });
        //# structTypes.ForEach(t => {
        //# var ename = Meta.GetXmlTypeName(t);
        //# var name = Meta.GetXmlTypeName("List<" + t + ">");
        public void Code__name__(ref List<__t__> value)
        {
            int count = CodeCount(ref value);
            if (count < 1) return;
            for (int i = 0; i < count; i++)
            {
                var m = default(__t__); Code__ename__(ref m); value.Add(m);
            }
        }

        //# });
        #endregion
    }
}
