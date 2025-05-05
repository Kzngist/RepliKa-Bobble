using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct Coordinates : IEquatable<Coordinates>
{
   [SerializeField] internal int K;
   [SerializeField] internal int Z;
   internal int N => -K - Z;

   internal const float scale = 1f;
   internal const float diagonalMultiplier = 1.1547005384f;

   public Coordinates(int k, int z)
   {
      K = k;
      Z = z;
   }
   
   /// <summary>
   ///   <para> Returns position of this coordinate </para>
   /// </summary>
   internal Vector3 GetPosition()
   {
      float y = -Z * (scale * diagonalMultiplier * 0.75f);
      float x = (K - N) / 2f * scale;
      return new Vector3(x, y, 0);
   }
   
   /// <summary>
   ///   <para> Creates coordinates from position </para>
   /// </summary>
   internal static Coordinates FromPosition(Vector3 position)
   {
      // for every horizontal unit, k += 1, n -= 1
      float xOffset = position.x / scale;
      float k = +xOffset;
      float n = -xOffset;
      
      // for every vertical unit, k += 0.5, n += 0.5,
      float yOffset = position.y / (scale * diagonalMultiplier * 0.75f);
      k += 0.5f * yOffset;
      n += 0.5f * yOffset;
      
      float z = -k - n;
      
      // rounding to nearest int
      int iK = Mathf.RoundToInt(k);
      int iZ = Mathf.RoundToInt(z);
      int iN = Mathf.RoundToInt(n);
      
      // check rounding errors and discard the axis with the largest rounding delta
      if (iK + iZ + iN != 0)
      {
         float dK = Mathf.Abs(iK - k);
         float dZ = Mathf.Abs(iZ - z);
         float dN = Mathf.Abs(iN - n);

         if (dK > dZ && dK > dN)
         {
            iK = -iZ - iN;
         }
         else if (dZ > dN)
         {
            iZ = -iK - iN;
         }
      }

      return new Coordinates(iK, iZ);
   }
   
   /// <summary>
   ///   <para> Creates a Coordinates from direction </para>
   /// </summary>
   internal static Coordinates FromDirection(Direction direction)
   {
      switch (direction)
      {
         case Direction.NE:
            return new Coordinates(1, -1);
         case Direction.E:
            return new Coordinates(1, 0);
         case Direction.SE:
            return new Coordinates(0, 1);
         case Direction.SW:
            return new Coordinates(-1, 1);
         case Direction.W:
            return new Coordinates(-1, 0);
         case Direction.NW:
            return new Coordinates(0, -1);
         default:
            DebugConsole.Instance.Log($"Unspecified direction", LogType.Warning);
            return new Coordinates(0, 0);
      }
   }

   internal static List<Coordinates> GetRing(Coordinates origin, int radius, Direction startDirection = Direction.NE)
   {
      if (radius == 0) return new List<Coordinates> {origin};

      List<Coordinates> coordinates = new();
      for (int i = 0; i < 6; i++)
      {
         Direction direction = startDirection.Rotate(-i);
         for (int j = 0; j < radius; j++)
         {
            coordinates.Add(radius * FromDirection(direction) + j * FromDirection(direction.Rotate(4)));
         }
      }

      // move first to last for better representations
      coordinates.Add(coordinates[0]);
      coordinates.RemoveAt(0);
      
      return coordinates;
   }
   
   public override string ToString()
   {
      return $"({K}, {Z}, {N})";
   }
   
   public static Coordinates operator +(Coordinates a) => a;
   public static Coordinates operator -(Coordinates a) => new Coordinates(-a.K, -a.Z);
   
   public static Coordinates operator +(Coordinates a, Coordinates b) => new Coordinates(a.K + b.K, a.Z + b.Z);
   public static Coordinates operator -(Coordinates a, Coordinates b) => a + (-b);

   public static Coordinates operator *(Coordinates a, int b) => new Coordinates(a.K * b, a.Z * b);
   public static Coordinates operator *(int b, Coordinates a) => new Coordinates(a.K * b, a.Z * b);
   public static Coordinates operator /(Coordinates a, int b)
   {
      if (b == 0) throw new DivideByZeroException();

      return new Coordinates(a.K / b, a.Z / b);
   }
   public static Coordinates operator /(int b, Coordinates a)
   {
      if (b == 0) throw new DivideByZeroException();

      return new Coordinates(a.K / b, a.Z / b);
   }

   public bool Equals(Coordinates other)
   {
      return K == other.K && Z == other.Z;
   }

   public override bool Equals(object obj)
   {
      return obj is Coordinates other && Equals(other);
   }

   public override int GetHashCode()
   {
      return HashCode.Combine(K, Z);
   }
}
