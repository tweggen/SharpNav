// Copyright (c) 2014 Robert Rouhani <robert.rouhani@gmail.com> and other contributors (see CONTRIBUTORS file).
// Licensed under the MIT License - https://raw.github.com/Robmaister/SharpNav/master/LICENSE

using System;
using System.Collections.Generic;

using SharpNav.Geometry;

namespace SharpNav
{
	//TODO right now this is basically an alias for TiledNavMesh. Fix this in the future.

	/// <summary>
	/// A TiledNavMesh generated from a collection of triangles and some settings
	/// </summary>
	public class NavMesh : TiledNavMesh
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NavMesh" /> class.
		/// </summary>
		/// <param name="builder">The NavMeshBuilder data</param>
		public NavMesh(NavMeshBuilder builder)
			: base(builder)
		{
		}

		/// <summary>
		/// Generates a <see cref="NavMesh"/> given a collection of triangles and some settings.
		/// </summary>
		/// <param name="triangles">The triangles that form the level.</param>
		/// <param name="settings">The settings to generate with.</param>
		/// <returns>A <see cref="NavMesh"/>.</returns>
		public static NavMesh Generate(IEnumerable<Triangle3> triangles, NavMeshGenerationSettings settings)
		{
			Console.WriteLine($"{DateTime.UtcNow}: GetBoundingBox()");
			BBox3 bounds = triangles.GetBoundingBox(settings.CellSize);
			var hf = new Heightfield(bounds, settings);
			hf.RasterizeTriangles(triangles);
			Console.WriteLine($"{DateTime.UtcNow}: RasterizeTriangles()");
			hf.FilterLedgeSpans(settings.VoxelAgentHeight, settings.VoxelMaxClimb);
			Console.WriteLine($"{DateTime.UtcNow}: FilterLedgeSpans()");
			hf.FilterLowHangingWalkableObstacles(settings.VoxelMaxClimb);
			Console.WriteLine($"{DateTime.UtcNow}: FilterLowHanging()");
			hf.FilterWalkableLowHeightSpans(settings.VoxelAgentHeight);

			var chf = new CompactHeightfield(hf, settings);
			Console.WriteLine($"{DateTime.UtcNow}: Erode()");
			chf.Erode(settings.VoxelAgentRadius);
			Console.WriteLine($"{DateTime.UtcNow}: BuildDistanceField()");
			chf.BuildDistanceField();
			Console.WriteLine($"{DateTime.UtcNow}: BuildRegions()");
			chf.BuildRegions(2, settings.MinRegionSize, settings.MergedRegionSize);

			Console.WriteLine($"{DateTime.UtcNow}: BuildContourSet()");
			var cont = chf.BuildContourSet(settings);

			Console.WriteLine($"{DateTime.UtcNow}: PolyMesh()");
			var polyMesh = new PolyMesh(cont, settings);

			Console.WriteLine($"{DateTime.UtcNow}: PolyMeshDetail()");
			var polyMeshDetail = new PolyMeshDetail(polyMesh, chf, settings);

			Console.WriteLine($"{DateTime.UtcNow}: NavMeshBuilder()");
			var buildData = new NavMeshBuilder(polyMesh, polyMeshDetail, new Pathfinding.OffMeshConnection[0], settings);

			Console.WriteLine($"{DateTime.UtcNow}: NavMesh()");
			var navMesh = new NavMesh(buildData);
			return navMesh;
		}
	}
}
