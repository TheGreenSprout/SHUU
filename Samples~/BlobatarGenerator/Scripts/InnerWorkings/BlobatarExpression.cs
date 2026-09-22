using System.Collections.Generic;

namespace SHUU.Samples.Blobatar.InnerWorkings
{
    public enum BlobatarExpr
    {
        Idle, Happy, Sad, Mad, Surprised, Wink, Sleepy, Smug, Unsure, Scared, Love, Shy, Sick, Thinking
    }

    /// <summary>
    /// A static facial pose: per-eye scale/offset/tilt deltas, a body bob offset,
    /// and (for the colorful expressions) how "hot" the mood tint is mixed in.
    /// `shake`/`rock` exist in the original for idle motion only and have no
    /// effect on the static render, so they're kept only for completeness.
    /// </summary>
    public struct Pose
    {
        public float esx, esy, tilt, edy, edx, esx2, esy2, tilt2, edy2, lockAmt, heat, shake, rock, bdy;
        public static readonly Pose Ident = new Pose { esx = 1f, esy = 1f };
    }

    public class ExpressionDef
    {
        public Pose pose;
        public Tint? tint;
    }

    /// <summary>The fourteen named expressions, and the pose-baking math from morph.ts. Ported from expression.ts.</summary>
    public static class BlobatarExpression
    {
        public static readonly Dictionary<BlobatarExpr, ExpressionDef> Roster = new Dictionary<BlobatarExpr, ExpressionDef>
        {
            [BlobatarExpr.Idle] = new ExpressionDef { pose = Pose.Ident, tint = null },

            [BlobatarExpr.Happy] = new ExpressionDef
            {
                pose = new Pose { esx = 1.72f, esy = 0.3f, tilt = 8f, edy = -1.5f, edx = 1.5f, esx2 = 0.08f, esy2 = 0.05f, tilt2 = -16f, lockAmt = 1f, bdy = -2.2f },
                tint = null,
            },
            [BlobatarExpr.Sad] = new ExpressionDef
            {
                pose = new Pose { esx = 0.6f, esy = 0.56f, tilt = 26f, edy = 3.6f, edx = 1.9f, esx2 = -0.05f, esy2 = -0.07f, tilt2 = -7f, lockAmt = 1f, bdy = 2.6f },
                tint = null,
            },
            [BlobatarExpr.Mad] = new ExpressionDef
            {
                pose = new Pose { esx = 1.85f, esy = 0.26f, tilt = -33f, edy = 0.4f, edx = 0.6f, esy2 = -0.03f, tilt2 = 5f, lockAmt = 1f, heat = 0.62f, shake = 0.55f, bdy = 0.8f },
                tint = BlobatarColor.HOT,
            },
            [BlobatarExpr.Surprised] = new ExpressionDef
            {
                pose = new Pose { esx = 1.34f, esy = 1.2f, tilt = -6f, edy = -1.05f, edx = 0.5f, esx2 = 0.05f, esy2 = 0.07f, tilt2 = 3f, lockAmt = 1f, bdy = -1.4f },
                tint = null,
            },
            [BlobatarExpr.Wink] = new ExpressionDef
            {
                pose = new Pose { esx = 1.32f, esy = 0.76f, tilt = 5f, edy = -0.6f, edx = 0.8f, esx2 = 0.26f, esy2 = -0.56f, tilt2 = -11f, lockAmt = 1f, bdy = -1.1f },
                tint = null,
            },
            [BlobatarExpr.Sleepy] = new ExpressionDef
            {
                pose = new Pose { esx = 1.14f, esy = 0.22f, tilt = 0f, edy = 2.4f, edx = 0.3f, esx2 = -0.04f, esy2 = 0.03f, tilt2 = 4f, lockAmt = 1f, bdy = 1.2f },
                tint = null,
            },
            [BlobatarExpr.Smug] = new ExpressionDef
            {
                pose = new Pose { esx = 1.3f, esy = 0.42f, tilt = 18f, edy = -0.5f, edx = 0.5f, esx2 = 0.06f, esy2 = -0.06f, tilt2 = -36f, lockAmt = 1f, bdy = -1f },
                tint = null,
            },
            [BlobatarExpr.Unsure] = new ExpressionDef
            {
                pose = new Pose { esx = 0.95f, esy = 1.02f, tilt = 4f, edy = -0.2f, edx = 0.3f, esx2 = 0.24f, esy2 = -0.44f, tilt2 = -18f, lockAmt = 1f, bdy = 0f },
                tint = null,
            },
            [BlobatarExpr.Scared] = new ExpressionDef
            {
                pose = new Pose { esx = 0.78f, esy = 0.96f, tilt = -12f, edy = -1.5f, edx = -0.8f, esx2 = -0.04f, esy2 = 0.05f, tilt2 = 4f, lockAmt = 1f, shake = 0.35f, bdy = -0.6f },
                tint = null,
            },
            [BlobatarExpr.Love] = new ExpressionDef
            {
                pose = new Pose { esx = 0.86f, esy = 1.28f, tilt = -14f, edy = -0.5f, edx = -0.35f, esx2 = 0.05f, esy2 = 0.06f, tilt2 = 6f, lockAmt = 1f, heat = 0.6f, bdy = -1.6f },
                tint = BlobatarColor.ROSE,
            },
            [BlobatarExpr.Shy] = new ExpressionDef
            {
                pose = new Pose { esx = 0.62f, esy = 0.5f, tilt = 10f, edy = 1.4f, edx = -0.2f, esx2 = -0.05f, esy2 = -0.04f, tilt2 = -8f, lockAmt = 1f, heat = 0.55f, bdy = 0.9f },
                tint = BlobatarColor.BLUSH,
            },
            [BlobatarExpr.Sick] = new ExpressionDef
            {
                pose = new Pose { esx = 1.25f, esy = 0.34f, tilt = 20f, edy = 1.8f, edx = 0.8f, esx2 = 0.05f, esy2 = -0.05f, tilt2 = -6f, lockAmt = 1f, heat = 0.6f, shake = 0.18f, bdy = 1.4f },
                tint = BlobatarColor.BILE,
            },
            [BlobatarExpr.Thinking] = new ExpressionDef
            {
                pose = new Pose { esx = 1.15f, esy = 0.62f, tilt = 0f, edy = 4.2f, edx = 0.4f, esx2 = 0.02f, esy2 = 0.06f, tilt2 = 0f, edy2 = -8.4f, lockAmt = 1f, rock = 0.8f, bdy = -0.4f },
                tint = null,
            },
        };

        /// <summary>
        /// Applies a pose to the (unposed) eye pair, returning the posed eyes plus
        /// the whole-body vertical offset. Ported from morph.ts's static bake path.
        /// </summary>
        public static (List<Eye> eyes, float bdy) BakePose(List<Eye> eyes, Pose p)
        {
            var baked = new List<Eye>(eyes.Count);
            for (int i = 0; i < eyes.Count; i++)
            {
                var e = eyes[i];
                bool second = i == 1;
                float side = second ? 1f : -1f;
                float cx = e.cx + p.edx * side;
                float cy = e.cy + p.edy + (second ? p.edy2 : 0f);
                float rx = e.rx * (p.esx + (second ? p.esx2 : 0f));
                float ry = e.ry * (p.esy + (second ? p.esy2 : 0f));
                float rot = e.rot * (1f - p.lockAmt) + (p.tilt + (second ? p.tilt2 : 0f)) * side;
                baked.Add(new Eye { cx = cx, cy = cy, rx = rx, ry = ry, n = e.n, rot = rot });
            }
            return (baked, p.bdy);
        }
    }
}
