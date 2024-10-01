// <copyright file="FileHelpers.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

namespace StreamerKit;

public static class FileHelpers
{
    public static string GetDataDirectory()
        => Path.Combine(System.AppContext.BaseDirectory, "data");
}