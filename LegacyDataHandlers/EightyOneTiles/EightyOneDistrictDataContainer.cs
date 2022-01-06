using ColossalFramework;
using ColossalFramework.IO;

namespace EManagersLib.LegacyDataHandlers.EightyOneTiles {
    public sealed class EightyOneDistrictDataContainer : IDataContainer {
        public void AfterDeserialize(DataSerializer s) { }

        public void Deserialize(DataSerializer s) {
            const byte Alpha1Default = 255;
            int i;
            DistrictManager dmInstance = Singleton<DistrictManager>.instance;
            DistrictManager.Cell[] districtGrid = dmInstance.m_districtGrid;
            DistrictManager.Cell[] parkGrid = dmInstance.m_parkGrid;
            int len = districtGrid.Length;
            EncodedArray.Byte @byte = EncodedArray.Byte.BeginRead(s);
            for (i = 0; i < len; i++) {
                districtGrid[i].m_district1 = @byte.Read();
            }
            for (i = 0; i < len; i++) {
                districtGrid[i].m_district2 = @byte.Read();
            }
            for (i = 0; i < len; i++) {
                districtGrid[i].m_district3 = @byte.Read();
            }
            for (i = 0; i < len; i++) {
                districtGrid[i].m_district4 = @byte.Read();
            }
            for (i = 0; i < len; i++) {
                districtGrid[i].m_alpha1 = @byte.Read();
            }
            for (i = 0; i < len; i++) {
                districtGrid[i].m_alpha2 = @byte.Read();
            }
            for (i = 0; i < len; i++) {
                districtGrid[i].m_alpha3 = @byte.Read();
            }
            for (i = 0; i < len; i++) {
                districtGrid[i].m_alpha4 = @byte.Read();
            }
            /* This is legacy repairer, as it seems to not have initialized alpha bytes */
            if (s.version == 2U) {
                for (i = 0; i < len; i++) {
                    if (districtGrid[i].m_district1 == 0x00) {
                        districtGrid[i].m_alpha1 = Alpha1Default;
                        districtGrid[i].m_alpha2 = 0x00;
                        districtGrid[i].m_alpha3 = 0x00;
                        districtGrid[i].m_alpha4 = 0x00;
                    }
                }
            }
            /* Now read in parkGrid data */
            if (s.version >= 2U) {
                for (i = 0; i < len; i++) {
                    parkGrid[i].m_district1 = @byte.Read();
                }
                for (i = 0; i < len; i++) {
                    parkGrid[i].m_district2 = @byte.Read();
                }
                for (i = 0; i < len; i++) {
                    parkGrid[i].m_district3 = @byte.Read();
                }
                for (i = 0; i < len; i++) {
                    parkGrid[i].m_district4 = @byte.Read();
                }
                for (i = 0; i < len; i++) {
                    parkGrid[i].m_alpha1 = @byte.Read();
                }
                for (i = 0; i < len; i++) {
                    parkGrid[i].m_alpha2 = @byte.Read();
                }
                for (i = 0; i < len; i++) {
                    parkGrid[i].m_alpha3 = @byte.Read();
                }
                for (i = 0; i < len; i++) {
                    parkGrid[i].m_alpha4 = @byte.Read();
                }
                /* This is legacy repairer, as it seems to not have initialized alpha bytes */
                if (s.version == 2U) {
                    for (i = 0; i < len; i++) {
                        if (parkGrid[i].m_district1 == 0x00) {
                            parkGrid[i].m_alpha1 = Alpha1Default;
                            parkGrid[i].m_alpha2 = 0x00;
                            parkGrid[i].m_alpha3 = 0x00;
                            parkGrid[i].m_alpha4 = 0x00;
                        }
                    }
                }
            } else {
                /* This is legacy repairer, as it seems to not have initialized alpha bytes */
                for (i = 0; i < len; i++) {
                    parkGrid[i].m_district1 = 0x00;
                }
                for (i = 0; i < len; i++) {
                    parkGrid[i].m_district2 = 0x00;
                }
                for (i = 0; i < len; i++) {
                    parkGrid[i].m_district3 = 0x00;
                }
                for (i = 0; i < len; i++) {
                    parkGrid[i].m_district4 = 0x00;
                }
                for (i = 0; i < len; i++) {
                    parkGrid[i].m_alpha1 = Alpha1Default;
                }
                for (i = 0; i < len; i++) {
                    parkGrid[i].m_alpha2 = 0x00;
                }
                for (i = 0; i < len; i++) {
                    parkGrid[i].m_alpha3 = 0x00;
                }
                for (i = 0; i < len; i++) {
                    parkGrid[i].m_alpha4 = 0x00;
                }
            }
            @byte.EndRead();
        }

        public void Serialize(DataSerializer s) {
            int i;
            DistrictManager dmInstance = Singleton<DistrictManager>.instance;
            DistrictManager.Cell[] districtGrid = dmInstance.m_districtGrid;
            DistrictManager.Cell[] parkGrid = dmInstance.m_parkGrid;
            int len = districtGrid.Length;
            EncodedArray.Byte @byte = EncodedArray.Byte.BeginWrite(s);
            for (i = 0; i < len; i++) {
                @byte.Write(districtGrid[i].m_district1);
            }
            for (i = 0; i < len; i++) {
                @byte.Write(districtGrid[i].m_district2);
            }
            for (i = 0; i < len; i++) {
                @byte.Write(districtGrid[i].m_district3);
            }
            for (i = 0; i < len; i++) {
                @byte.Write(districtGrid[i].m_district4);
            }
            for (i = 0; i < len; i++) {
                @byte.Write(districtGrid[i].m_alpha1);
            }
            for (i = 0; i < len; i++) {
                @byte.Write(districtGrid[i].m_alpha2);
            }
            for (i = 0; i < len; i++) {
                @byte.Write(districtGrid[i].m_alpha3);
            }
            for (i = 0; i < len; i++) {
                @byte.Write(districtGrid[i].m_alpha4);
            }
            for (i = 0; i < len; i++) {
                @byte.Write(parkGrid[i].m_district1);
            }
            for (i = 0; i < len; i++) {
                @byte.Write(parkGrid[i].m_district2);
            }
            for (i = 0; i < len; i++) {
                @byte.Write(parkGrid[i].m_district3);
            }
            for (i = 0; i < len; i++) {
                @byte.Write(parkGrid[i].m_district4);
            }
            for (i = 0; i < len; i++) {
                @byte.Write(parkGrid[i].m_alpha1);
            }
            for (i = 0; i < len; i++) {
                @byte.Write(parkGrid[i].m_alpha2);
            }
            for (i = 0; i < len; i++) {
                @byte.Write(parkGrid[i].m_alpha3);
            }
            for (i = 0; i < len; i++) {
                @byte.Write(parkGrid[i].m_alpha4);
            }
            @byte.EndWrite();
        }
    }
}
