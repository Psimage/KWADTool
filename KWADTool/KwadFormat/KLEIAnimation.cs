using System.IO;
using System.Text;

namespace KWADTool.KwadFormat
{
    public class KLEIAnimation : KLEIResource
    {
        public static readonly byte[] KLEI_TYPE = Encoding.ASCII.GetBytes("ANM1"); //WARNING: mutable

        private readonly byte[] signature;

        public byte[] GetSignature()
        {
            return (byte[])signature.Clone();
        }

        public uint StructSize { get; private set; }

        public uint AnimCount { get; private set; }
        private readonly Animation[] animations;
        public Animation[] GetAnimations()
        {
            return (Animation[])animations.Clone();
        }

        public uint FrameCount { get; private set; }
        private readonly Frame[] frames;
        public Frame[] GetFrames()
        {
            return (Frame[])frames.Clone();
        }

        public uint EventCount { get; private set; }
        private readonly uint[] events;
        public uint[] GetEvents()
        {
            return (uint[])events.Clone();
        }

        public uint InstanceCount { get; private set; }
        private readonly Instance[] instances;
        public Instance[] GetInstances()
        {
            return (Instance[])instances.Clone();
        }

        public uint ColourCount { get; private set; }
        private readonly Colour[] colours;
        public Colour[] GetColours()
        {
            return (Colour[])colours.Clone();
        }

        public uint TransformCount { get; private set; }
        private readonly USAffine3D[] transforms;
        public USAffine3D[] GetTransforms()
        {
            return (USAffine3D[])transforms.Clone();
        }

        public uint EventStringsSize { get; private set; }
        private readonly char[] eventStrings;
        public char[] GetEventStrings()
        {
            return (char[])eventStrings.Clone();
        }

        public KLEIAnimation(BinaryReader reader)
        {
            signature = reader.ReadBytes(8);
            StructSize = reader.ReadUInt32();

            AnimCount = reader.ReadUInt32();
            animations = new Animation[AnimCount];
            for (var i = 0; i < AnimCount; i++)
            {
                animations[i] = new Animation(reader);
            }

            FrameCount = reader.ReadUInt32();
            frames = new Frame[FrameCount];
            for (var i = 0; i < FrameCount; i++)
            {
                frames[i] = new Frame(reader);
            }

            EventCount = reader.ReadUInt32();
            events = new uint[EventCount];
            for (var i = 0; i < EventCount; i++)
            {
                events[i] = reader.ReadUInt32();
            }

            InstanceCount = reader.ReadUInt32();
            instances = new Instance[InstanceCount];
            for (var i = 0; i < InstanceCount; i++)
            {
                instances[i] = new Instance(reader);
            }

            ColourCount = reader.ReadUInt32();
            colours = new Colour[ColourCount];
            for (var i = 0; i < ColourCount; i++)
            {
                colours[i] = new Colour(reader);
            }

            TransformCount = reader.ReadUInt32();
            transforms = new USAffine3D[TransformCount];
            for (var i = 0; i < TransformCount; i++)
            {
                transforms[i] = new USAffine3D(reader);
            }

            EventStringsSize = reader.ReadUInt32();
            eventStrings = reader.ReadChars((int) EventStringsSize);
        }

        public class Animation
        {
            public int NameHash { get; private set; }
            
            private readonly char[] name;
            public char[] GetName()
            {
                return (char[]) name.Clone();
            }

            public string GetNameString()
            {
                return new string(name);
            }

            public uint RootSymbolHash { get; private set; }
            public float FrameRate { get; private set; }
            public uint FacingMask { get; private set; }
            public uint FrameIdx { get; private set; }
            public uint FrameCount { get; private set; }

            public Animation(BinaryReader reader)
            {
                NameHash = reader.ReadInt32();
                name = reader.ReadChars(20);
                RootSymbolHash = reader.ReadUInt32();
                FrameRate = reader.ReadSingle();
                FacingMask = reader.ReadUInt32();
                FrameIdx = reader.ReadUInt32();
                FrameCount = reader.ReadUInt32();
            }
        }

        public class Instance
        {
            public uint SymbolHash { get; private set; }
            public uint FolderHash { get; private set; }
            public uint ParentHash { get; private set; }
            public uint SymbolFrame { get; private set; }
            public uint ParentTransformIdx { get; private set; }
            public uint TransformIdx { get; private set; }
            // ReSharper disable once InconsistentNaming
            public uint CMIdx { get; private set; }
            // ReSharper disable once InconsistentNaming
            public uint CAIdx { get; private set; }

            public Instance(BinaryReader reader)
            {
                SymbolHash = reader.ReadUInt32();
                FolderHash = reader.ReadUInt32();
                ParentHash = reader.ReadUInt32();
                SymbolFrame = reader.ReadUInt32();
                ParentTransformIdx = reader.ReadUInt32();
                TransformIdx = reader.ReadUInt32();
                CMIdx = reader.ReadUInt32();
                CAIdx = reader.ReadUInt32();
            }
        }

        public class Frame
        {
            public uint EventIdx { get; private set; }
            public uint EventCount { get; private set; }
            public uint InstanceIdx { get; private set; }
            public uint InstanceCount { get; private set; }

            public Frame(BinaryReader reader)
            {
                EventIdx = reader.ReadUInt32();
                EventCount = reader.ReadUInt32();
                InstanceIdx = reader.ReadUInt32();
                InstanceCount = reader.ReadUInt32();
            }
        }

        public class Colour
        {
            public float R { get; private set; }
            public float G { get; private set; }
            public float B { get; private set; }
            public float A { get; private set; }

            public Colour(BinaryReader reader)
            {
                R = reader.ReadSingle();
                G = reader.ReadSingle();
                B = reader.ReadSingle();
                A = reader.ReadSingle();
            }
        }
        ////KLEI_AMINATION --------------------------------------------------------
        //typedef struct {
        //    byte signature[8]<open=suppress, bgcolor=0xF4A903>; //KLEIANM1
        //    u32 structSize <format=hex>; //including signature and this field (in bytes)
    
        //    u32 animCount<hidden=true>;


        //    u32 frameCount<hidden=true>;

    
        //    u32 eventCount<hidden=true>;
        //    u32 events[eventCount]; //array of indexes that point to the strings (null-teminated)
        //                            //in eventStrings char array

        //    u32 instanceCount<hidden=true>;


        //    u32 colourCount<hidden=true>;


        //    u32 transformCount<hidden=true>;
        //    USAffine3D transforms[transformCount];

        //    u32 eventStringsSize<hidden=true>;
        //    char eventStrings[eventStringsSize];

        //} KLEI_ANM<size=KLEI_ANM_Size, bgcolor=0xFEF5E1>; 
    }
}
