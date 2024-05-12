using System;
using System.Collections.Generic;
using Il2Cpp;

namespace CMS21Together.Shared.Data
{
    [Serializable]
    public class ModPartScript
    {
        public string id;
        public string tunedID;
        public bool isExamined;
        public bool isPainted;
        public ModPaintData paintData;
        public int paintType;
        public ModColor color;
        public int quality;
        public float condition;
        public float dust;
        public List<String> bolts;
        public bool unmounted;

        public int partID;
        public int partIdNumber;
        public ModPartType type;

        public Guid GUID;

        public ModPartScript(PartScript data, int _partID, int _partIdNumber, ModPartType _type)
        {
            this.id = data.id;
            this.tunedID = data.tunedID;
            this.isExamined = data.IsExamined;
            this.isPainted = data.IsPainted;
            this.paintData = new ModPaintData(data.CurrentPaintData);
            this.paintType = (int)data.CurrentPaintType;
            this.color = new ModColor(data.currentColor);
            this.quality = data.Quality;
            this.condition = data.Condition;
            this.dust = data.Dust;
            this.unmounted = data.IsUnmounted;
            
            this.partID = _partID;
            this.partIdNumber = _partIdNumber;
            this.type = _type;

            this.GUID = new Guid();
        }

        public PartScript ToGame()
        {
            PartScript data = new PartScript();
            
            data.id = this.id;
            data.tunedID = this.tunedID;
            data.IsExamined = this.isExamined;
            data.IsPainted = this.isPainted;
            data.CurrentPaintData = this.paintData.ToGame(this.paintData);
            data.CurrentPaintType = (PaintType)this.paintType;
            data.currentColor = ModColor.ToColor(this.color);
            data.Quality = this.quality;
            data.Condition = this.condition;
            data.Dust = this.dust;
            data.IsUnmounted = this.unmounted;

            return data;
        }
    }
}