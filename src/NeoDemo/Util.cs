﻿using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Veldrid.NeoDemo
{
    internal static class Util
    {
        internal static uint SizeInBytes<T>(this T[] array) where T : struct
        {
            return (uint)(array.Length * Unsafe.SizeOf<T>());
        }

        // Code adapted from https://bitbucket.org/sinbad/ogre/src/9db75e3ba05c/OgreMain/include/OgreVector3.h
        internal static Quaternion FromToRotation(Vector3 from, Vector3 to, Vector3 fallbackAxis = default(Vector3))
        {
            // Based on Stan Melax's article in Game Programming Gems
            Quaternion q;
            // Copy, since cannot modify local
            Vector3 v0 = from;
            Vector3 v1 = to;
            v0 = Vector3.Normalize(v0);
            v1 = Vector3.Normalize(v1);

            float d = Vector3.Dot(v0, v1);
            // If dot == 1, vectors are the same
            if (d >= 1.0f)
            {
                return Quaternion.Identity;
            }
            if (d < (1e-6f - 1.0f))
            {
                if (fallbackAxis != Vector3.Zero)
                {
                    // rotate 180 degrees about the fallback axis
                    q = Quaternion.CreateFromAxisAngle(fallbackAxis, (float)Math.PI);
                }
                else
                {
                    // Generate an axis
                    Vector3 axis = Vector3.Cross(Vector3.UnitX, from);
                    if (axis.LengthSquared() == 0) // pick another if colinear
                    {
                        axis = Vector3.Cross(Vector3.UnitY, from);
                    }

                    axis = Vector3.Normalize(axis);
                    q = Quaternion.CreateFromAxisAngle(axis, (float)Math.PI);
                }
            }
            else
            {
                float s = (float)Math.Sqrt((1 + d) * 2);
                float invs = 1.0f / s;

                Vector3 c = Vector3.Cross(v0, v1);

                q.X = c.X * invs;
                q.Y = c.Y * invs;
                q.Z = c.Z * invs;
                q.W = s * 0.5f;
                q = Quaternion.Normalize(q);
            }
            return q;
        }

        // modifies projection matrix in place
        // clipPlane is in camera space
        public static void CalculateObliqueMatrixPerspective(ref Matrix4x4 projection, Vector4 clipPlane)
        {
            bool result = Matrix4x4.Invert(projection, out Matrix4x4 invProj);
            Debug.Assert(result);
            Vector4 q = Vector4.Transform(
                new Vector4(Math.Sign(clipPlane.X), Math.Sign(clipPlane.Y), 1.0f, 1.0f),
                invProj);
            Vector4 c = clipPlane * (2.0F / (Vector4.Dot(clipPlane, q)));
            projection.M13 = c.X;
            projection.M23 = c.Y;
            projection.M33 = c.Z;
            projection.M43 = c.W - 1.0F;
        }
    }
}
