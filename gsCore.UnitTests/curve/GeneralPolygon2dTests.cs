using Microsoft.VisualStudio.TestTools.UnitTesting;
using g3;
using System;
using System.Collections.Generic;
using System.Text;
using gsCore.UnitTests;

namespace g3.UnitTests
{
    [TestClass()]
    public class GeneralPolygon2dTests
    {     
        [TestMethod()]
        public void TrimSegment2d_SegmentEndsOutside()
        {
            // Arrange
            var square = new GeneralPolygon2d(Polygon2d.MakeRectangle(new Vector2d(-1, 0), new Vector2d(1, 1)));
            var seg = new Segment2d(new Vector2d(0, -1), new Vector2d(0, 2));

            // Act
            var result = square.TrimSegment2d(seg);

            // Assert
            Assert.AreEqual(1, result.Count);
            AssertExtensions.AreEqual(new Vector2d(0, 0), result[0].P0);
            AssertExtensions.AreEqual(new Vector2d(0, 1), result[0].P1);
        }

        [TestMethod()]
        public void TrimSegment2d_SegmentEndsOnBoundary()
        {
            // Arrange
            var square = new GeneralPolygon2d(Polygon2d.MakeRectangle(new Vector2d(-1, 0), new Vector2d(1, 1)));
            var seg = new Segment2d(new Vector2d(0, 0), new Vector2d(0, 1));

            // Act
            var result = square.TrimSegment2d(seg);

            // Assert
            Assert.AreEqual(1, result.Count);
            AssertExtensions.AreEqual(new Vector2d(0, 0), result[0].P0);
            AssertExtensions.AreEqual(new Vector2d(0, 1), result[0].P1);
        }

        [TestMethod()]
        public void TrimSegment2d_SegmentEndsInside()
        {
            // Arrange
            var square = new GeneralPolygon2d(Polygon2d.MakeRectangle(new Vector2d(-1, 0), new Vector2d(1, 1)));
            var seg = new Segment2d(new Vector2d(0, 0.1), new Vector2d(0, 0.8));

            // Act
            var result = square.TrimSegment2d(seg);

            // Assert
            Assert.AreEqual(1, result.Count);
            AssertExtensions.AreEqual(new Vector2d(0, 0.1), result[0].P0);
            AssertExtensions.AreEqual(new Vector2d(0, 0.8), result[0].P1);
        }

        [TestMethod()]
        public void TrimSegment2d_SegmentEndsPartial()
        {
            // Arrange
            var square = new GeneralPolygon2d(Polygon2d.MakeRectangle(new Vector2d(-1, 0), new Vector2d(1, 1)));
            var seg = new Segment2d(new Vector2d(0, 0.1), new Vector2d(0, 2));

            // Act
            var result = square.TrimSegment2d(seg);

            // Assert
            Assert.AreEqual(1, result.Count);
            AssertExtensions.AreEqual(new Vector2d(0, 0.1), result[0].P0);
            AssertExtensions.AreEqual(new Vector2d(0, 1), result[0].P1);
        }

        [TestMethod()]
        public void TrimSegment2d_SegmentAcrossHole()
        {
            // Arrange
            var square = new GeneralPolygon2d(Polygon2d.MakeRectangle(new Vector2d(0, 0), 4, 4));
            var hole = Polygon2d.MakeRectangle(new Vector2d(0, 0), 2, 2);
            hole.Reverse();
            square.AddHole(hole);
            var seg = new Segment2d(new Vector2d(0, -4), new Vector2d(0, 4));

            // Act
            var result = square.TrimSegment2d(seg);

            // Assert
            Assert.AreEqual(2, result.Count);
            AssertExtensions.AreEqual(new Vector2d(0, -2), result[0].P0);
            AssertExtensions.AreEqual(new Vector2d(0, -1), result[0].P1);
            AssertExtensions.AreEqual(new Vector2d(0, 1), result[1].P0);
            AssertExtensions.AreEqual(new Vector2d(0, 2), result[1].P1);
        }

        [TestMethod()]
        public void TrimSegment2d_SegmentCollinear()
        {
            // Checks that when the segment is coincident with segments of the polygon,
            // segments that lie on polygon edges are not included, as per the intended
            // behavior.

            // Arrange
            var square = new GeneralPolygon2d(new Polygon2d(new double[] {
                0, 0,
                3, 0,
                3, 1,
                2, 1,
                2, 1.3,
                2, 1.6,
                2, 2,
                3, 2,
                3, 3,
                0, 3,
            }));
            var seg = new Segment2d(new Vector2d(2, 0.5), new Vector2d(2, 2.5));

            // Act
            var result = square.TrimSegment2d(seg);

            // Assert
            Assert.AreEqual(2, result.Count);
            AssertExtensions.AreEqual(new Vector2d(2, 0.5), result[0].P0);
            AssertExtensions.AreEqual(new Vector2d(2, 1.0), result[0].P1);
            AssertExtensions.AreEqual(new Vector2d(2, 2.0), result[1].P0);
            AssertExtensions.AreEqual(new Vector2d(2, 2.5), result[1].P1);
        }
    }
}