using UnityEngine;

public enum Direction
{
   NE,
   E,
   SE,
   SW,
   W,
   NW
}

public static class DirectionExtensions
{
   public static Direction Opposite(this Direction direction)
   {
      return Rotate(direction, 3);
   }
   
   public static Direction Next(this Direction direction)
   {
      return Rotate(direction, 1);
   }
   
   public static Direction Prev(this Direction direction)
   {
      return Rotate(direction, -1);
   }
   
   public static Direction Rotate(this Direction direction, int amount)
   {
      return (Direction)((int)direction + amount).Mod(6);
   }
   
   /// <summary>
   /// Returns the closest Direction from a rotation
   /// </summary>
   internal static Direction ToDirection(this Quaternion rotation)
   {
      return (Direction) Mathf.RoundToInt(rotation.eulerAngles.z / 60f);
   }
   
   /// <summary>
   /// Returns a rotation from direction
   /// </summary>
   internal static Quaternion ToRotation(this Direction direction)
   {
      return Quaternion.Euler(0, 0, (int) direction * 60f);
   }
}