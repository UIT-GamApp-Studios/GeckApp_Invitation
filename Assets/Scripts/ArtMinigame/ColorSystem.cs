using UnityEngine;

public enum ColorId
{
    // Base palette
    Red, White, Blue, Black, Yellow,
    // Mix results
    Pink, DarkRed, Orange, Purple, SkyBlue, DarkBlue, Green, Cream, Olive,
    None // Used for Empty state
}

public static class ColorRecipeDatabase
{
    private static readonly (ColorId a, ColorId b, ColorId result)[] Recipes = {
        (ColorId.Red, ColorId.White, ColorId.Pink),
        (ColorId.Red, ColorId.Black, ColorId.DarkRed),
        (ColorId.Red, ColorId.Yellow, ColorId.Orange),
        (ColorId.Red, ColorId.Blue, ColorId.Purple),
        (ColorId.Blue, ColorId.White, ColorId.SkyBlue),
        (ColorId.Blue, ColorId.Black, ColorId.DarkBlue),
        (ColorId.Blue, ColorId.Yellow, ColorId.Green),
        (ColorId.Yellow, ColorId.White, ColorId.Cream),
        (ColorId.Yellow, ColorId.Black, ColorId.Olive)
    };

    public static bool TryGetMixResult(ColorId inputA, ColorId inputB, out ColorId result)
    {
        foreach (var recipe in Recipes)
        {
            if ((recipe.a == inputA && recipe.b == inputB) || 
                (recipe.a == inputB && recipe.b == inputA))
            {
                result = recipe.result;
                return true;
            }
        }
        
        result = ColorId.None;
        return false;
    }
}