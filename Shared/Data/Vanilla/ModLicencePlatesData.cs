﻿using System;
using Il2Cpp;

namespace CMS21Together.Shared.Data.Vanilla;

[Serializable]
public class ModLicencePlatesData
{
    public string LicensePlateNumberFront;
    public string LicensePlateNumberRear;
    public string FactoryLicensePlateNumber;
    public string LicensePlateFrontTex;
    public string LicensePlateRearTex;

    public ModLicencePlatesData(LicensePlatesData data)
    {
        LicensePlateNumberFront = data.LicensePlateNumberFront;
        LicensePlateNumberRear = data.LicensePlateNumberRear;
        LicensePlateFrontTex = data.LicensePlateFrontTex;
        LicensePlateRearTex = data.LicensePlateRearTex;
        FactoryLicensePlateNumber = data.FactoryLicensePlateNumber;
    }

    public LicensePlatesData ToGame()
    {
        LicensePlatesData data = new LicensePlatesData();
        data.FactoryLicensePlateNumber = FactoryLicensePlateNumber;
        data.LicensePlateFrontTex = LicensePlateFrontTex;
        data.LicensePlateRearTex = LicensePlateRearTex;
        data.LicensePlateNumberFront = LicensePlateNumberFront;
        data.LicensePlateRearTex = LicensePlateRearTex;

        return data;
    }
}