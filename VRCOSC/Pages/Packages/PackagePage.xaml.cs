﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows.Input;
using Semver;
using VRCOSC.Packages;

namespace VRCOSC.Pages.Packages;

public partial class PackagePage
{
    private readonly PackageViewModel packageViewModel = new();

    public PackagePage()
    {
        InitializeComponent();

        PackageGrid.DataContext = packageViewModel;
    }

    private void PackagePage_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        packageViewModel.Packages.Add(new Package("Test2", new SemVersion(1, 1, 0), new SemVersion(1, 0, 0), PackageType.Community));
    }
}
