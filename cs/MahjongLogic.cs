using System;
using System.Collections.Generic;
using System.Linq;

namespace Share
{
    using Card = Int32;
    using Cards = List<int>;
    using Color = Byte;
    using Num = Byte;
    using Eigen = Int64;

    public class MahjongValue
    {
        //麻将相关
        public const int INVALID_CARD = 99;
        public const byte COLOR_INVALID = 99;
        public const byte COLOR_WAN = 0;
        public const byte COLOR_TONG = 1;
        public const byte COLOR_TIAO = 2;
        public const byte COLOR_FENG = 3;
        public const byte COLOR_JIAN = 4;
        public const byte COLOR_HUA = 5;
        public const byte COLOR_JOKER = 6;
        public const byte LAIZI_COUNT = 7;
        public const byte CARD_COUNT = 8;

        public static List<List<byte>> VALID_NUMS = new List<List<byte>>()
        {
            new List<byte>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
            new List<byte>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
            new List<byte>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
            new List<byte>() { 1, 2, 3, 4 },
            new List<byte>() { 1, 2, 3 },
            new List<byte>() { 1, 2, 3, 4, 5, 6, 7, 8 },
            new List<byte>() { 1 }
        };

        public static Dictionary<OPE, short> OP_WEIGHT_MAP = new Dictionary<OPE, short>()
        {
            [OPE.GANGSHANG_HUA] = 5,
            [OPE.QIANGGANG_HU] = 5,
            [OPE.BAHUA_HU] = 5,
            [OPE.SANJINDAO] = 5,
            [OPE.ZIMO] = 4,
            [OPE.HU] = 4,
            [OPE.MING_GANG] = 3,
            [OPE.AN_GANG] = 3,
            [OPE.BU_GANG] = 3,
            [OPE.PENG] = 2,
            [OPE.LEFT_CHI] = 1,
            [OPE.MIDDLE_CHI] = 1,
            [OPE.RIGHT_CHI] = 1,
            [OPE.PASS] = 0,
            [OPE.OUTCARD] = 0,
            [OPE.SWAPCARDS] = 0
        };

        public static Dictionary<int, string> CardNameMap = new Dictionary<int, string>()
        {
            [0x01] = "1万",
            [0x02] = "2万",
            [0x03] = "3万",
            [0x04] = "4万",
            [0x05] = "5万",
            [0x06] = "6万",
            [0x07] = "7万",
            [0x08] = "8万",
            [0x09] = "9万",
            [0x11] = "1筒",
            [0x12] = "2筒",
            [0x13] = "3筒",
            [0x14] = "4筒",
            [0x15] = "5筒",
            [0x16] = "6筒",
            [0x17] = "7筒",
            [0x18] = "8筒",
            [0x19] = "9筒",
            [0x21] = "1条",
            [0x22] = "2条",
            [0x23] = "3条",
            [0x24] = "4条",
            [0x25] = "5条",
            [0x26] = "6条",
            [0x27] = "7条",
            [0x28] = "8条",
            [0x29] = "9条",
            [0x31] = "东",
            [0x32] = "南",
            [0x33] = "西",
            [0x34] = "北",
            [0x41] = "中",
            [0x42] = "发",
            [0x43] = "白",
            [0x51] = "春",
            [0x52] = "夏",
            [0x53] = "秋",
            [0x54] = "冬",
            [0x55] = "梅",
            [0x56] = "兰",
            [0x57] = "竹",
            [0x58] = "菊",
            [0x61] = "JOKER",
            [INVALID_CARD] = "无效牌"
            //[0x99] = "癞子"
        };

        public static Dictionary<int, string> OpNameMap = new Dictionary<int, string>()
        {
            [(int)OPE.PASS] = "过",
            [(int)OPE.LEFT_CHI] = "左吃",
            [(int)OPE.MIDDLE_CHI] = "中吃",
            [(int)OPE.RIGHT_CHI] = "右吃",
            [(int)OPE.PENG] = "碰",

            [(int)OPE.MING_GANG] = "明杠",
            [(int)OPE.AN_GANG] = "暗杠",
            [(int)OPE.BU_GANG] = "补杠",
            [(int)OPE.HU] = "胡",
            [(int)OPE.ZIMO] = "自摸",
            [(int)OPE.QIANGGANG_HU] = "抢杠胡",
            [(int)OPE.GANGSHANG_HUA] = "杠上开花",
            [(int)OPE.SWAPCARDS] = "换三张",
        };

        public static Dictionary<MainFan, int> main_fanshu_config_map = new Dictionary<MainFan, int>()
        {
            [MainFan.JiuLianBaoDeng] = 64,
            [MainFan.ShiBaLuoHan] = 64,
            [MainFan.LianQiDui] = 64,
            [MainFan.QingLongQiDui] = 32,
            [MainFan.DiHu] = 32,
            [MainFan.TianHu] = 32,
            [MainFan.QingYaoJiu] = 16,
            [MainFan.QingQiDui] = 16,
            [MainFan.LongQiDui] = 8,
            [MainFan.QingDui] = 8,
            [MainFan.QiDui] = 4,
            [MainFan.QingYiSe] = 4,
            [MainFan.PengPengHu] = 2,
            [MainFan.PingHu] = 1,
        };

        public static Dictionary<MainFan, string> main_fan_name_map = new Dictionary<MainFan, string>()
        {
            [MainFan.JiuLianBaoDeng] = "九莲宝灯",
            [MainFan.ShiBaLuoHan] = "十八罗汉",
            [MainFan.LianQiDui] = "连七对",
            [MainFan.QingLongQiDui] = "青龙七队",
            [MainFan.DiHu] = "地胡",
            [MainFan.TianHu] = "天胡",
            [MainFan.QingYaoJiu] = "清幺九",
            [MainFan.QingQiDui] = "清七对",
            [MainFan.LongQiDui] = "龙七对",
            [MainFan.QingDui] = "清对",
            [MainFan.QiDui] = "七对",
            [MainFan.QingYiSe] = "清一色",
            [MainFan.PengPengHu] = "碰碰胡",
            [MainFan.PingHu] = "平胡",
        };

        public static Dictionary<ExtraFan, int> extra_fanshu_config_map = new Dictionary<ExtraFan, int>()
        {
            [ExtraFan.MiaoShouHuiChun] = 8,
            [ExtraFan.HaiDiLaoYue] = 8,
            [ExtraFan.GangShangKaiHua] = 4,
            [ExtraFan.QiangGangHu] = 4,
            [ExtraFan.QuanQiuRen] = 4,
            [ExtraFan.DaiYaoJiu] = 2,
            [ExtraFan.GangShangPao] = 2,
            [ExtraFan.Gen] = 2,
            [ExtraFan.ZiMo] = 1,
            [ExtraFan.MenQing] = 1
        };

        public static Dictionary<ExtraFan, string> extra_fan_name_map = new Dictionary<ExtraFan, string>()
        {
            [ExtraFan.MiaoShouHuiChun] = "妙手回春",
            [ExtraFan.HaiDiLaoYue] = "海底捞月",
            [ExtraFan.GangShangKaiHua] = "杠上开花",
            [ExtraFan.QiangGangHu] = "抢杠胡",
            [ExtraFan.QuanQiuRen] = "全求人",
            [ExtraFan.DaiYaoJiu] = "带幺九",
            [ExtraFan.GangShangPao] = "杠上炮",
            [ExtraFan.Gen] = "根",
            [ExtraFan.ZiMo] = "自摸",
            [ExtraFan.MenQing] = "门清",
        };
    }

    public enum PairType
    {
        MianZi,
        DaZi,
        Guzhang,
    }

    public class MahjongLogic
    {
        public static long[] EIGEN_MAP = {
            0x0_000_000_000,
            0x1_000_000_001,
            0x1_000_000_010,
            0x1_000_000_100,
            0x1_000_001_000,
            0x1_000_010_000,
            0x1_000_100_000,
            0x1_001_000_000,
            0x1_010_000_000,
            0x1_100_000_000,
        };

        public static long[] SHIFT_MAP = {
            0, 0, //1
            4, //2
            8, 12, 16, 20, 24, 28, 32, //9
            36,
        };

        public static long[] KEZI_ARR = {
            0,
            0x3_000_000_003,
            0x3_000_000_030,
            0x3_000_000_300,
            0x3_000_003_000,
            0x3_000_030_000,
            0x3_000_300_000,
            0x3_003_000_000,
            0x3_030_000_000,
            0x3_300_000_000,
        };

        public static long[] SHUNZI_ARR = {
            0,
            0x3_000_000_111,
            0x3_000_001_110,
            0x3_000_011_100,
            0x3_000_111_000,
            0x3_001_110_000,
            0x3_011_100_000,
            0x3_111_000_000,
        };

        public static long[] EYES_ARR = {
            0,
            0x2_000_000_002,
            0x2_000_000_020,
            0x2_000_000_200,
            0x2_000_002_000,
            0x2_000_020_000,
            0x2_000_200_000,
            0x2_002_000_000,
            0x2_020_000_000,
            0x2_200_000_000,
        };

        static long[] DAZI_ARR = {
            0x2_000000011,
            0x2_110000000, // 边张
            0x2_000000110,
            0x2_000001100,
            0x2_000011000,
            0x2_000110000,
            0x2_001100000,
            0x2_011000000, // 两面
            0x2_000000101,
            0x2_000001010,
            0x2_000010100,
            0x2_000101000,
            0x2_001010000,
            0x2_010100000,
            0x2_101000000, // 嵌张
            0x2_000000002,
            0x2_000000020,
            0x2_000000200,
            0x2_000002000,
            0x2_000020000,
            0x2_000200000,
            0x2_002000000,
            0x2_020000000,
            0x2_200000000, // 对子
        };

        static long[] FENG_MIANZI_ARR = {
            0x3_000_000_003,
            0x3_000_000_030,
            0x3_000_000_300,
            0x3_000_003_000,
        };

        static long[] FENG_DAZI_ARR = { 0x2000000002, 0x2000000020, 0x2000000200, 0x2000002000 };
        static long[] JIAN_MIANZI_ARR = { 0x3_000_000_003, 0x3_000_000_030, 0x3_000_000_300 };
        static long[] JIAN_DAZI_ARR = { 0x2000000002, 0x2000000020, 0x2000000200 };

        static long[] MIANZI_ARR = {
            0x3_000_000_003,
            0x3_000_000_030,
            0x3_000_000_300,
            0x3_000_003_000,
            0x3_000_030_000,
            0x3_000_300_000,
            0x3_003_000_000,
            0x3_030_000_000,
            0x3_300_000_000,
            0x3_000_000_111,
            0x3_000_001_110,
            0x3_000_011_100,
            0x3_000_111_000,
            0x3_001_110_000,
            0x3_011_100_000,
            0x3_111_000_000,
        };

        static Dictionary<long, short> REVERSE_EIGEN_MAP = new Dictionary<Eigen, short>()
        {
            { 0x0_000000000, 0 },
            { 0x1_000000001, 1 },
            { 0x1_000000010, 2 },
            { 0x1_000000100, 3 },
            { 0x1_000001000, 4 },
            { 0x1_000010000, 5 },
            { 0x1_000100000, 6 },
            { 0x1_001000000, 7 },
            { 0x1_010000000, 8 },
            { 0x1_100000000, 9 },
        };

        static Dictionary<long, double> DAZI_WEIGHT_MAP = new Dictionary<Eigen, double>()
        {
            { 0x2_000000011, 0.7 },
            { 0x2_110000000, 0.7 },
            { 0x2_000000110, 0.8 },
            { 0x2_000001100, 0.8 },
            { 0x2_000011000, 0.8 },
            { 0x2_000110000, 0.8 },
            { 0x2_001100000, 0.8 },
            { 0x2_011000000, 0.8 },
            { 0x2_000000101, 0.7 },
            { 0x2_000001010, 0.7 },
            { 0x2_000010100, 0.7 },
            { 0x2_000101000, 0.7 },
            { 0x2_001010000, 0.7 },
            { 0x2_010100000, 0.7 },
            { 0x2_101000000, 0.7 },
            { 0x2_000000002, 0.9 },
            { 0x2_000000020, 0.9 },
            { 0x2_000000200, 0.9 },
            { 0x2_000002000, 0.9 },
            { 0x2_000020000, 0.9 },
            { 0x2_000200000, 0.9 },
            { 0x2_002000000, 0.9 },
            { 0x2_020000000, 0.9 },
            { 0x2_200000000, 0.9 },
        };

        static Dictionary<long, List<byte>> DAZI_TING_NUM_MAP = new Dictionary<Eigen, List<Color>>()
        {
            { 0x2_000_000_011, new List<Color>() { 3 } },
            { 0x2_110_000_000, new List<Color>() { 7 } },
            { 0x2_000_000_110, new List<Color>() { 1, 4 } },
            { 0x2_000_001_100, new List<Color>() { 2, 5 } },
            { 0x2_000_011_000, new List<Color>() { 3, 6 } },
            { 0x2_000_110_000, new List<Color>() { 4, 7 } },
            { 0x2_001_100_000, new List<Color>() { 5, 8 } },
            { 0x2_011_000_000, new List<Color>() { 6, 9 } },
            { 0x2_000_000_101, new List<Color>() { 2 } },
            { 0x2_000_001_010, new List<Color>() { 3 } },
            { 0x2_000_010_100, new List<Color>() { 4 } },
            { 0x2_000_101_000, new List<Color>() { 5 } },
            { 0x2_001_010_000, new List<Color>() { 6 } },
            { 0x2_010_100_000, new List<Color>() { 7 } },
            { 0x2_101_000_000, new List<Color>() { 8 } },
            { 0x2_000_000_002, new List<Color>() { 1 } },
            { 0x2_000_000_020, new List<Color>() { 2 } },
            { 0x2_000_000_200, new List<Color>() { 3 } },
            { 0x2_000_002_000, new List<Color>() { 4 } },
            { 0x2_000_020_000, new List<Color>() { 5 } },
            { 0x2_000_200_000, new List<Color>() { 6 } },
            { 0x2_002_000_000, new List<Color>() { 7 } },
            { 0x2_020_000_000, new List<Color>() { 8 } },
            { 0x2_200_000_000, new List<Color>() { 9 } },
        };

        static Dictionary<long, double> MIANZI_WEIGHT_MAP = new Dictionary<Eigen, double>()
        {
            { 0x3_000000003, 1.1 },
            { 0x3_000000030, 1.1 },
            { 0x3_000000300, 1.1 },
            { 0x3_000003000, 1.1 },
            { 0x3_000030000, 1.1 },
            { 0x3_000300000, 1.1 },
            { 0x3_003000000, 1.1 },
            { 0x3_030000000, 1.1 },
            { 0x3_300000000, 1.1 },
            { 0x3_000000111, 1.1 },
            { 0x3_000001110, 1.1 },
            { 0x3_000011100, 1.1 },
            { 0x3_000111000, 1.1 },
            { 0x3_001110000, 1.1 },
            { 0x3_011100000, 1.1 },
            { 0x3_111000000, 1.1 },
        };


        public static List<List<List<long>>> DEIGENS_MAP = new List<List<List<Eigen>>>()
        {
            new List<List<Eigen>>() { MIANZI_ARR.ToList(), DAZI_ARR.ToList() },
            new List<List<Eigen>>() { MIANZI_ARR.ToList(), DAZI_ARR.ToList() },
            new List<List<Eigen>>() { MIANZI_ARR.ToList(), DAZI_ARR.ToList() },
            new List<List<Eigen>>() { FENG_MIANZI_ARR.ToList(), FENG_DAZI_ARR.ToList() },
            new List<List<Eigen>>() { JIAN_MIANZI_ARR.ToList(), JIAN_DAZI_ARR.ToList() },
            new List<List<Eigen>>() { MIANZI_ARR.ToList(), DAZI_ARR.ToList() },
        };

        public static Dictionary<Eigen, int> LAIZI_HUMAP;
        private static Random rand = new Random();

        //static Dictionary<Eigen, int> FENG_LAIZI_HUMAP = MahjongLogic.gen_laizimap(HumapType.FENG, 4);

        public static void Init(Dictionary<Eigen, int> laizi_map)
        {
            LAIZI_HUMAP = laizi_map;
        }

        static Dictionary<Eigen, int> gen_humap(int humap_type)
        {
            int max_handcard_count = 17;
            Dictionary<Eigen, int> m = new Dictionary<Eigen, Card>();

            var max_num = humap_type == 0 ? 9 : 4;

            for (int mianzi_cnt = 1; mianzi_cnt <= (max_handcard_count - 2) / 3; mianzi_cnt++)
            {
                List<Eigen> data = new List<Eigen>() { 0 }; //0这里代表没有面子的情况
                for (int num = 1; num <= max_num; num++)
                {
                    data.Add(KEZI_ARR[num]);
                }

                if (humap_type == 0)
                {
                    for (int _ = 0; _ <= mianzi_cnt; _++)
                    {
                        for (int num = 1; num <= 7; num++)
                        {
                            data.Add(SHUNZI_ARR[num]);
                        }
                    }
                };

                Action<List<Eigen>> callback = (select_data) =>
                {
                    var select_data_eigen = select_data.Sum();
                    if (m.ContainsKey(select_data_eigen) == false)
                    {

                        //foreach (var v in EYES_ARR[0..(max_num + 1)])
                        foreach (var v in EYES_ARR.Take(max_num + 1))
                        {
                            select_data_eigen += v;
                            if (is_valid_cardtype(select_data_eigen))
                            {
                                m[select_data_eigen] = 0;
                                //插入结果表
                            }
                            select_data_eigen -= v;
                        }
                    }
                };

                ShareFunc.Combine(data, 0, new List<Eigen>(), mianzi_cnt, callback);
            }

            return m;
        }

        public static Dictionary<Eigen, int> gen_laizimap(int humap_type, int target_que_count)
        {
            int max_num = humap_type == 0 ? 9 : 4;

            var humap = gen_humap(humap_type);
            var m = new Dictionary<Eigen, int>(humap);

            Func<Dictionary<Eigen, int>, int, Dictionary<Eigen, int>> gen_sub_map = delegate (Dictionary<Eigen, int> humap, int que_count)
            {
                var new_m = new Dictionary<Eigen, int>();

                //foreach (var (eigen, old_que_count) in humap)
                foreach (var item in humap)
                {

                    Eigen eigen = item.Key;
                    Card old_que_count = item.Value;
                    if (old_que_count + 1 > que_count)
                    {
                        continue;
                    }

                    for (Num num = 1; num <= max_num; num++)
                    {
                        if (get_num_cardcount(eigen, num) >= 1)
                        {
                            var new_eigen = eigen - EIGEN_MAP[num];
                            if (new_eigen == 0)
                            {
                                continue;
                            }

                            int value;
                            if (m.TryGetValue(new_eigen, out value))
                            {
                                if (value > old_que_count + 1)
                                {
                                    m.Add(new_eigen, old_que_count + 1);
                                    new_m.Add(new_eigen, old_que_count + 1);
                                }
                            }
                            else
                            {
                                m.Add(new_eigen, old_que_count + 1);
                                new_m.Add(new_eigen, old_que_count + 1);
                            }
                        }
                    }
                }
                return new_m;
            };

            int que_count = 1;
            while (que_count <= target_que_count)
            {
                humap = gen_sub_map(humap, que_count);
                que_count += 1;
            }

            return m;
        }

        static bool is_valid_cardtype(Eigen eigen)
        {
            for (Num num = 1; num <= 9; num++)
            {
                if (get_num_cardcount(eigen, num) > 4)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool is_eigen_include(Eigen eigen, Eigen deigen)
        {
            return eigen < deigen ? false : ((eigen - deigen) & 0x888888888) == 0;
        }

        public static byte get_card_num(int card)
        {
            return (byte)(card & 0xF);
        }

        public static byte get_card_color(int card)
        {
            return (byte)(card >> 4);
        }

        public static int make_card(byte color, byte num)
        {
            return (byte)((color << 4) + num);
        }

        public Eigen[] get_new_matrix()
        {
            return new Eigen[]
            {
                0, //万
                0, //筒
                0, //条
                0, //风
                0, //箭
                0, //花
                0, //Joker
                0, //癞子数目
                0  //牌数目
            };
        }

        public static bool belongToHu(OPE opcode)
        {
            return opcode == OPE.HU || opcode == OPE.ZIMO || opcode == OPE.GANGSHANG_HUA || opcode == OPE.QIANGGANG_HU
                || opcode == OPE.SANJINDAO || opcode == OPE.BAHUA_HU;
        }
        public static bool belongToMoHu(OPE opcode)
        {
            return opcode == OPE.ZIMO || opcode == OPE.BAHUA_HU || opcode == OPE.GANGSHANG_HUA || opcode == OPE.SANJINDAO;
        }

        public static bool belongToChiHu(OPE opcode)
        {
            return opcode == OPE.HU || opcode == OPE.QIANGGANG_HU;
        }

        public static bool belongToGang(OPE opcode)
        {
            return opcode == OPE.AN_GANG || opcode == OPE.MING_GANG || opcode == OPE.BU_GANG;
        }

        public static bool belongToChi(OPE opcode)
        {
            return opcode == OPE.LEFT_CHI || opcode == OPE.MIDDLE_CHI || opcode == OPE.RIGHT_CHI;
        }

        public static bool cards_equal(Cards cards1, Cards cards2)
        {
            return cards1.Count == cards2.Count && cards1.Count(t => !cards2.Contains(t)) == 0;
        }

        public static bool cards_include(Cards cards1, Cards cards2)
        {
            var cards1_copy = cards1.ToList();
            return cards2.TrueForAll(t =>
            {
                var include = cards1_copy.IndexOf(t);
                if (include != -1)
                {
                    cards1_copy.RemoveAt(include);
                    return true;
                }
                else
                {
                    return false;
                }
            });
        }

        public HashSet<int> m_laizis;
        byte[] m_valid_colors;
        bool contain_qidui;

        public MahjongLogic()
        {
            m_laizis = new HashSet<int> {
                0x61,
            };
            m_valid_colors = new byte[]
            {
                Share.MahjongValue.COLOR_WAN,
                Share.MahjongValue.COLOR_TONG,
                Share.MahjongValue.COLOR_TIAO,
            };
            contain_qidui = true;
        }

        public void set_laizis(Cards cards)
        {
            m_laizis = new HashSet<Card>(cards);
        }

        public bool is_laizi(int card)
        {
            return m_laizis.Contains(card);
        }
        public static bool is_number_color(byte color)
        {
            return color <= 2;
        }

        static public Cards get_new_card_walls(List<Color> colors)
        {
            Cards card_walls = new Cards();

            foreach (var color in colors)
            {
                if (color == Share.MahjongValue.COLOR_WAN)
                    card_walls.AddRange(new Cards() { 0x01, 0x01, 0x01, 0x01, 0x02, 0x02, 0x02, 0x02, 0x03, 0x03, 0x03, 0x03, 0x04, 0x04, 0x04, 0x04, 0x05,
                 0x05, 0x05, 0x05, 0x06, 0x06, 0x06, 0x06, 0x07, 0x07, 0x07, 0x07, 0x08, 0x08, 0x08, 0x08, 0x09, 0x09,
                 0x09, 0x09 });
                else if (color == Share.MahjongValue.COLOR_TONG)
                {
                    card_walls.AddRange(new Cards() { 0x11, 0x11, 0x11, 0x11, 0x12, 0x12, 0x12, 0x12, 0x13, 0x13, 0x13, 0x13, 0x14, 0x14, 0x14, 0x14, 0x15,
                 0x15, 0x15, 0x15, 0x16, 0x16, 0x16, 0x16, 0x17, 0x17, 0x17, 0x17, 0x18, 0x18, 0x18, 0x18, 0x19, 0x19,
                 0x19, 0x19 });
                }
                else if (color == Share.MahjongValue.COLOR_TIAO)
                {
                    card_walls.AddRange(new Cards() { 0x21, 0x21, 0x21, 0x21, 0x22, 0x22, 0x22, 0x22, 0x23, 0x23, 0x23, 0x23, 0x24, 0x24, 0x24, 0x24, 0x25,
                     0x25, 0x25, 0x25, 0x26, 0x26, 0x26, 0x26, 0x27, 0x27, 0x27, 0x27, 0x28, 0x28, 0x28, 0x28, 0x29, 0x29,
                     0x29, 0x29 });
                }
                else if (color == Share.MahjongValue.COLOR_FENG)
                {
                    card_walls.AddRange(new Cards() { 0x31, 0x31, 0x31, 0x31, 0x32, 0x32, 0x32, 0x32, 0x33, 0x33, 0x33, 0x33, 0x34,
                                            0x34, 0x34, 0x34 });
                }
                else if (color == Share.MahjongValue.COLOR_JIAN)
                {
                    card_walls.AddRange(new Cards() { 0x41, 0x41, 0x41, 0x41, 0x42, 0x42, 0x42, 0x42, 0x43, 0x43, 0x43, 0x43 });

                }
                else if (color == Share.MahjongValue.COLOR_HUA)
                {
                    card_walls.AddRange(new Cards() { 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58 });

                }
                else if (color == Share.MahjongValue.COLOR_JOKER)
                {
                    card_walls.AddRange(new Cards() { 0x61, 0x61, 0x61, 0x61 });
                }
            }

            return card_walls;
        }

        public void matrix_add_card(long[] m, Card card)
        {
            var num = get_card_num(card);
            var color = get_card_color(card);
            m[color] += EIGEN_MAP[num];

            if (is_laizi(card))
            {
                m[Share.MahjongValue.LAIZI_COUNT] += 1;
            }

            m[Share.MahjongValue.CARD_COUNT] += 1;
        }

        public void matrix_remove_card(long[] m, Card card)
        {
            var num = get_card_num(card);
            var color = get_card_color(card);

            if (is_eigen_include(m[color], EIGEN_MAP[num]) == false)
            {
                throw new Exception("手牌矩阵中不包含该牌:" + card);
            }

            m[color] -= EIGEN_MAP[num];
            if (is_laizi(card))
            {
                m[Share.MahjongValue.LAIZI_COUNT] -= 1;
            }
            m[Share.MahjongValue.CARD_COUNT] -= 1;
        }

        public long[] cards_to_matrix(Cards cards)
        {
            var m = get_new_matrix();
            cards.ForEach(v => matrix_add_card(m, v));
            return m;
        }


        //获取num对应的eigen
        public static Eigen get_num_eigen(Num num)
        {
            return EIGEN_MAP[num];
        }

        //获取矩阵手牌牌的总数
        public static int get_total_cardcount(long[] m)
        {
            return (int)m[Share.MahjongValue.CARD_COUNT];
        }

        public static int get_laizi_cardcount(long[] m)
        {
            return (int)m[Share.MahjongValue.LAIZI_COUNT];
        }

        //获取特征值手牌牌的总数
        public static int get_eigen_cardcount(Eigen eigen)
        {
            return (int)(eigen >> 36);
        }

        //取特征值某一位的值（表示牌的数量）
        public static int get_num_cardcount(Eigen eigen, Num num)
        {
            return (int)(eigen >> (int)SHIFT_MAP[num] & 0xF);
        }

        public static int is_eigen_match(Eigen color_eigen)
        {
            int need = 99;
            LAIZI_HUMAP.TryGetValue(color_eigen, out need);
            return need;
        }

        public long[] shelter_laizi(long[] m)
        {
            if (m_laizis.Count == 0)
            {
                return m;
            }

            var new_m = m.ToArray();
            foreach (var card in m_laizis)
            {
                var color = get_card_color(card);
                var num = get_card_num(card);
                var count = get_num_cardcount(m[color], num);
                new_m[color] -= EIGEN_MAP[num] * count;
            }
            return new_m;
        }

        public Cards get_4_same_cards(long[] m)
        {
            Cards cards = new Cards();
            foreach (var color in m_valid_colors)
            {
                if ((m[color] & 0x444_444_444) != 0)
                {
                    foreach (var num in Share.MahjongValue.VALID_NUMS[color])
                    {
                        if (get_num_cardcount(m[color], num) == 4)
                        {
                            cards.Add(make_card(color, num));
                        }
                    }
                }
            }
            return cards;
        }

        public Cards get_3_same_cards(long[] m, bool contain_gen)
        {
            Cards cards = new Cards();
            foreach (var color in m_valid_colors)
            {
                foreach (var num in Share.MahjongValue.VALID_NUMS[color])
                {
                    var count = get_num_cardcount(m[color], num);
                    if (count == 3 || contain_gen && count == 4)
                    {
                        cards.Add(make_card(color, num));
                    }
                }
            }
            return cards;
        }

        //获取“根”的数量 （根即手牌有4张）
        public int get_gen_count(long[] m, bool contain_laizi = true)
        {
            var gen_count = 0;
            foreach (var color in m_valid_colors)
            {
                var color_eigen = m[color];

                //计算有效位1的数量

                var count = 0;
                var x = color_eigen & 0x444_444_444;
                while (x != 0)
                {
                    x &= (x - 1);
                    count++;
                }

                //计算有效位1的数量
                gen_count += count;
            }

            if (contain_laizi)
            {
                var kezis = get_3_same_cards(m, false);
                var laizi_count = get_laizi_cardcount(m);
                var possible_gen_count = 0;
                //var fake_times = Math.Min(kezis.Count, laizi_count);
                if (laizi_count > 0)
                {
                    foreach (var kezi_card in kezis)
                    {
                        m[Share.MahjongValue.LAIZI_COUNT]--;
                        m[Share.MahjongValue.CARD_COUNT] -= 1;

                        var res = test_hu(m, kezi_card);
                        if (res)
                        {
                            possible_gen_count++;
                        }

                        m[Share.MahjongValue.LAIZI_COUNT]++;
                        m[Share.MahjongValue.CARD_COUNT] += 1;
                    }
                    gen_count += Math.Min(possible_gen_count, laizi_count);
                }
            }

            return gen_count;
        }

        public bool without_shunzi(long[] m)
        {
            var count_kezi = 0;
            var count_jiang = 0;
            int count_n = (get_total_cardcount(m) - 2) / 3;

            foreach (var color in m_valid_colors)
            {
                foreach (var num in Share.MahjongValue.VALID_NUMS[color])
                {
                    var count = get_num_cardcount(m[color], num);
                    if (count == 3)
                    {
                        count_kezi++;
                    }
                    else if (count == 2)
                    {
                        count_jiang++;
                    }
                }
            }
            return count_jiang == 1 && count_n == count_kezi;
        }

        public bool all_cards_are_same(Cards cards)
        {
            if (cards.Count == 0)
            {
                return false;
            }

            var card = cards[0];
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] != card)
                {
                    return false;
                }
            }
            return true;
        }

        //生成十八罗汉
        public List<Cards> gen_random_shibaluohan()
        {
            List<Cards> allcards = new List<Cards>();

            return allcards;
        }

        public List<Cards> gen_random_jiulianbaodeng()
        {
            List<Cards> allcards = new List<Cards>();
            return allcards;
        }

        public List<Cards> gen_random_lianqidui()
        {
            var allcards = new List<Cards>()
            {
                new Cards()
            };
            var alternative_cards = new Cards()
            {
                0x01, 0x02, 0x03,
                0x11, 0x12, 0x13,
                0x21, 0x22, 0x23,
            };

            var start_card = alternative_cards[rand.Next(0, alternative_cards.Count)];
            for (int i = 0; i < 7; i++)
            {
                allcards[allcards.Count - 1].Add(start_card + i);
                allcards[allcards.Count - 1].Add(start_card + i);
            }

            return allcards;
        }

        public List<Cards> gen_random_qidui()
        {
            var allcards = new List<Cards>();
            var alternative_cards = new Cards()
            {
                0x01, 0x01, 0x02, 0x02, 0x03, 0x03, 0x04, 0x04, 0x05, 0x05, 0x06, 0x06, 0x07, 0x07, 0x08, 0x08, 0x09, 0x09,
                0x11, 0x11, 0x12, 0x12, 0x13, 0x13, 0x14, 0x14, 0x15, 0x15, 0x16, 0x16, 0x17, 0x17, 0x18, 0x18, 0x19, 0x19,
                0x21, 0x21, 0x22, 0x22, 0x23, 0x23, 0x24, 0x24, 0x25, 0x25, 0x26, 0x26, 0x27, 0x27, 0x28, 0x28, 0x29, 0x29,
            };

            var start_card = alternative_cards[rand.Next(0, alternative_cards.Count)];
            for (int i = 0; i < 7; i++)
            {
                allcards[allcards.Count - 1].Add(start_card + i);
                allcards[allcards.Count - 1].Add(start_card + i);
            }
            return allcards;
        }

        public List<Cards> gen_random_pengpenghu()
        {
            List<Cards> allcards = new List<Cards>();
            return allcards;
        }

        public List<Cards> gen_random_daiyaojiu()
        {
            List<Cards> allcards = new List<Cards>();

            return allcards;
        }

        public List<Cards> gen_random_pinghu()
        {
            List<Cards> allcards = new List<Cards>();
            return allcards;
        }

        public List<Cards> gen_random_hu_cards()
        {
            //int fanshu = 0;
            List<Cards> allcards = new List<Cards>();

            //十八罗汉
            if (ShareFunc.Exp(0.01))
            {
                return gen_random_shibaluohan();
            }
            //九莲宝灯
            else if (ShareFunc.Exp(0.02))
            {
                return gen_random_jiulianbaodeng();
            }
            //连七对
            else if (ShareFunc.Exp(0.05))
            {
                return gen_random_lianqidui();
            }
            //七对
            else if (ShareFunc.Exp(0.1))
            {
                return gen_random_qidui();
            }
            //碰碰胡
            else if (ShareFunc.Exp(0.1))
            {
                return gen_random_pengpenghu();
            }
            //带幺九
            else if (ShareFunc.Exp(0.6))
            {
                return gen_random_daiyaojiu();
            }

            return gen_random_pinghu();
        }

        public (MainFan, List<ExtraFan>) calc_cards_fans(Cards handcards, List<Cards> opcards)
        {
            var handcards_m = cards_to_matrix(handcards);

            var allopcards = new Cards();
            opcards.ForEach(v => allopcards.AddRange(v));

            var alllcards = handcards.Concat(allopcards).ToList();
            var allcards_m = cards_to_matrix(alllcards);

            //var alllcards_without_chi = handcards.Concat(allopcards.Distinct).ToList();
            //var alllcards_without_chi_m = cards_to_matrix(alllcards_without_chi);

            var (is_qingyise, qingyise_color) = this.is_qingyise(allcards_m);
            var is_qidui = this.is_qidui(handcards_m);
            var gen_count = this.get_gen_count(handcards_m, true);
            var is_daiyaojiu = this.is_daiyaojiu(allcards_m);
            var gang_count = opcards.FindAll(v => v.Count == 4).Count;
            var is_jiulianbaodeng = this.is_jiulianbaodeng(handcards_m);

            var without_shunzi = this.without_shunzi(handcards_m);
            var all_opcards_are_same = opcards.TrueForAll(v => all_cards_are_same(v));
            var is_pengpenghu = all_opcards_are_same && without_shunzi;

            var is_lianqidui = this.is_lianqidui(handcards_m);

            MainFan main_fan = MainFan.PingHu;
            if (is_jiulianbaodeng)
            {
                main_fan = MainFan.JiuLianBaoDeng;
            }
            else if (gang_count == 4)
            {
                main_fan = MainFan.ShiBaLuoHan;
            }
            else if (is_lianqidui)
            {
                main_fan = MainFan.LianQiDui;
            }
            else if (is_qingyise && is_qidui && gen_count > 0)
            {
                main_fan = MainFan.QingLongQiDui;
            }
            else if (is_qingyise && is_daiyaojiu)
            {
                main_fan = MainFan.QingYaoJiu;
            }
            else if (is_qingyise && is_qidui)
            {
                main_fan = MainFan.QingQiDui;
            }
            else if (is_qidui && gen_count > 0)
            {
                main_fan = MainFan.LongQiDui;
            }
            else if (is_pengpenghu && is_qingyise)
            {
                main_fan = MainFan.QingDui;
            }
            else if (is_qidui)
            {
                main_fan = MainFan.QiDui;
            }
            else if (is_qingyise)
            {
                main_fan = MainFan.QingYiSe;
            }
            else if (is_pengpenghu)
            {
                main_fan = MainFan.PengPengHu;
            }

            var extra_fans = new List<ExtraFan>();

            //带幺九
            if (is_daiyaojiu && is_qingyise == false)
            {
                extra_fans.Add(ExtraFan.DaiYaoJiu);
            }

            //杠加番
            if (main_fan != MainFan.ShiBaLuoHan)
            {
                if (gang_count > 0)
                {
                    for (int i = 0; i < gang_count; i++)
                    {
                        extra_fans.Add(ExtraFan.Gen);
                    }
                }
            }

            return (main_fan, extra_fans);
        }

        public Dictionary<Card, (bool is_ting_any, Cards ting_cards)> get_ting_pairs(long[] m)
        {
            Dictionary<Card, (bool, Cards)> ting_pairs = new Dictionary<Card, (bool, Cards)>();
            foreach (var color in m_valid_colors)
            {
                var color_eigen = m[color];
                if (color_eigen == 0)
                {
                    continue;
                }

                foreach (var num in Share.MahjongValue.VALID_NUMS[color])
                {
                    var count = get_num_cardcount(color_eigen, num);
                    if (count > 0)
                    {
                        var card = make_card(color, num);
                        matrix_remove_card(m, card);
                        if (is_ting_any(m))
                        {
                            ting_pairs.Add(card, (true, new Cards()));
                        }
                        else
                        {
                            var ting_cards = get_ting_cards(m);
                            if (ting_cards.Count > 0)
                            {
                                ting_pairs.Add(card, (false, ting_cards));
                            }
                        }
                        matrix_add_card(m, card);
                    }
                }
            }

            return ting_pairs;
        }

        public Cards get_ting_cards(long[] m)
        {
            Cards ting_cards = new Cards();
            if ((get_total_cardcount(m) + 1) % 3 != 2)
            {
                return ting_cards;
            } //要嘛加上一张牌可以凑成3N牌型（不带将）要嘛凑成3N+2（包含七对的14张）

            if (contain_qidui)
            {
                var qidui_ting_cards = get_ting_qidui_card(m);
                ting_cards.AddRange(qidui_ting_cards);
            }

            //各种牌型的次数
            var count_2 = 0; //将
            List<Color> ting_colors = new List<Color>();
            var total_need = 0;

            //满足3N+2的在这里计算
            foreach (var color in m_valid_colors)
            {
                var color_eigen = m[color];
                //跳过没有牌的花色
                if (color_eigen == 0)
                {
                    continue;
                }
                var color_cardcount = get_eigen_cardcount(color_eigen);
                var color_need = is_eigen_match(color_eigen);

                if (color_need == 99)
                {
                    return ting_cards;
                }

                total_need += color_need;
                var total_lack = total_need - get_laizi_cardcount(m);
                if (total_lack - 1 > 0)
                {
                    return ting_cards;
                }

                if ((color_cardcount + color_need) % 3 == 2)
                {
                    count_2 += 1;
                }

                if (total_lack - 1 > 1 - count_2)
                {
                    return ting_cards;
                }

                ting_colors.Add(color);
            }

            //逐个试
            foreach (var color in ting_colors)
            {
                var color_eigen = m[color];

                var color_cardcount = get_eigen_cardcount(m[color]);
                var borrow_count_2 = 0;

                var color_need = is_eigen_match(color_eigen);

                if ((color_cardcount + color_need) % 3 == 2)
                {
                    borrow_count_2 = 1;
                    count_2 -= borrow_count_2;
                }

                foreach (var num in Share.MahjongValue.VALID_NUMS[color])
                {
                    //添加完牌之后的新特征值
                    var new_eigen = m[color] + EIGEN_MAP[num];
                    var new_cardcount = color_cardcount + 1;

                    var new_color_need = is_eigen_match(new_eigen);
                    var new_total_need = total_need - color_need + new_color_need;

                    if (new_color_need == 99)
                    {
                        continue;
                    }

                    var total_lack = new_total_need - get_laizi_cardcount(m);
                    if (total_lack > 0)
                    {
                        continue;
                    }

                    var new_count_2 = count_2 + (new_cardcount + new_color_need) % 3 / 2;
                    if (total_lack > 1 - new_count_2)
                    {
                        continue;
                    }

                    ting_cards.Add(make_card(color, num));
                }

                count_2 += borrow_count_2;
            }

            //七对和3N+2的计算结果去重
            return ting_cards.Distinct().ToList();
        }

        public bool match_shape(long[] m, int max_lack)
        {
            //各种牌型的次数
            var count_2 = 0; //将
            var total_need = 0; //需要的癞子数量

            //满足3N+2的在这里计算
            foreach (var color in m_valid_colors)
            {
                var color_eigen = m[color];
                //跳过没有牌的花色
                if (color_eigen == 0)
                {
                    continue;
                }

                var color_cardcount = get_eigen_cardcount(color_eigen);
                var color_need = is_eigen_match(color_eigen);

                if (color_need == 99)
                {
                    return false;
                }

                total_need += color_need;

                var total_lack = total_need - get_laizi_cardcount(m);

                if (total_lack > max_lack)
                {
                    return false;
                }

                //计算将个数
                if ((color_cardcount + color_need) % 3 == 2)
                {
                    count_2 += 1;
                }

                if (total_lack > max_lack + 1 - count_2)
                {
                    return false;
                }
            }
            //满足胡牌队列
            return true;
        }


        public bool is_ting(long[] m)
        {
            if (get_total_cardcount(m) % 3 != 1)
            {
                return false;
            }
            // 七对判断
            if (contain_qidui && is_ting_qidui(m))
            {
                return true;
            }

            return match_shape(m, 1);
        }

        public bool is_ting_any(long[] m)
        {
            if (get_total_cardcount(m) % 3 != 1)
            {
                return false;
            }
            if (m[Share.MahjongValue.LAIZI_COUNT] == 0)
            {
                return false;
            }
            m[Share.MahjongValue.LAIZI_COUNT] -= 1;
            m[Share.MahjongValue.CARD_COUNT] -= 1;
            var res = is_complete(m);
            m[Share.MahjongValue.LAIZI_COUNT] += 1;
            m[Share.MahjongValue.CARD_COUNT] += 1;
            return res;
        }
        public bool is_complete(long[] m)
        {
            return match_shape(m, 0);
        }

        public bool is_hu(long[] m)
        {
            if (get_total_cardcount(m) % 3 != 2)
            {
                return false;
            }

            // 七对判断
            if (contain_qidui && is_qidui(m))
            {
                return true;
            }

            return match_shape(m, 0);
        }

        public bool test_hu(long[] m, Card card)
        {
            matrix_add_card(m, card);
            var res = is_hu(m);
            matrix_remove_card(m, card);
            return res;
        }

        public bool match_qidui_shape(long[] m, int max_lack)
        {
            var total_need = 0;

            foreach (var color in m_valid_colors)
            {
                foreach (var num in Share.MahjongValue.VALID_NUMS[color])
                {
                    total_need += get_num_cardcount(m[color], num) & 0x1;
                }
            }

            if (total_need - get_laizi_cardcount(m) > max_lack)
            {
                return false;
            }

            return true;
        }

        public bool is_ting_qidui(long[] m)
        {
            var total_cardcount = get_total_cardcount(m);
            if (total_cardcount != 13)
            {
                return false;
            }

            return match_qidui_shape(m, 1);
        }

        //属于七对
        public bool is_qidui(long[] m)
        {
            var total_cardcount = get_total_cardcount(m);
            if (total_cardcount != 14)
            {
                return false;
            }

            if (m[Share.MahjongValue.LAIZI_COUNT] == 0)
            {
                foreach (var color in m_valid_colors)
                {
                    var color_eigen = m[color];
                    if ((color_eigen & 0x111_111_111) != 0)
                    {
                        return false;
                    }
                }
            }
            else
            {
                return match_qidui_shape(m, 0);
            }

            return true;
        }

        //获取听七对牌
        public Cards get_ting_qidui_card(long[] m)
        {
            var ting_cards = new Cards();
            var total_cardcount = get_total_cardcount(m);
            if (total_cardcount != 13)
            {
                return ting_cards;
            }

            if (m[Share.MahjongValue.LAIZI_COUNT] == 0)
            {
                byte possible_qidui_num = 0;
                var possible_qidui_color = Share.MahjongValue.COLOR_INVALID;

                foreach (var color in m_valid_colors)
                {
                    var color_eigen = m[color];
                    if (color_eigen == 0)
                    {
                        continue;
                    }

                    var bitand_res = color_eigen & 0x111_111_111;
                    if (bitand_res != 0)
                    {
                        //有重复的潜在听七对的数字，本牌型肯定不听七对
                        if (possible_qidui_color != Share.MahjongValue.COLOR_INVALID)
                        {
                            return ting_cards;
                        }

                        if ((bitand_res & (bitand_res - 1)) == 0)
                        {
                            var key = bitand_res + 0x1000_000_000;
                            short x;
                            if (REVERSE_EIGEN_MAP.TryGetValue(key, out x))
                            {
                                possible_qidui_num = (byte)x;
                                possible_qidui_color = color;
                            }
                            else
                            {
                                // possible_qidui_num = 0;
                                throw new Exception("计算错误，EIGEN_MAP不包含bitand_res {}");
                            }
                        }
                        else
                        {
                            return ting_cards;
                        }
                    }
                }

                if (possible_qidui_color != Share.MahjongValue.COLOR_INVALID)
                {
                    ting_cards.Add(make_card(possible_qidui_color, possible_qidui_num));
                    return ting_cards;
                }
            }
            else
            {
                if (is_ting_qidui(m))
                {
                    foreach (var color in m_valid_colors)
                    {
                        foreach (var num in Share.MahjongValue.VALID_NUMS[color])
                        {
                            if ((get_num_cardcount(m[color], num) & 0x1) == 1)
                            {
                                ting_cards.Add(make_card(color, num));
                            }
                        }
                    }
                    return ting_cards;
                }
            }

            return ting_cards;
        }

        //判断是否属于十三幺
        public bool is_shisanyao(long[] m)
        {
            var total_cardcount = get_total_cardcount(m);
            if (total_cardcount != 14)
            {
                return false;
            }

            foreach (var color in m_valid_colors)
            {
                var color_eigen = m[color];

                long match_eigen = 0;
                if (color == Share.MahjongValue.COLOR_WAN || color == Share.MahjongValue.COLOR_TONG || color == Share.MahjongValue.COLOR_TIAO)
                {
                    match_eigen = 0x100_000_001;
                }
                else if (color == Share.MahjongValue.COLOR_FENG)
                {
                    match_eigen = 0x1111;
                }
                else if (color == Share.MahjongValue.COLOR_JIAN)
                {
                    match_eigen = 0x111;
                }

                if (is_eigen_include(color_eigen, match_eigen) == false)
                {
                    return false;
                }
            };

            return true;
        }

        public Card get_ting_shisanyao_card(long[] m)
        {
            return Share.MahjongValue.INVALID_CARD;
        }

        public (bool, Color) is_qingyise(long[] m)
        {
            var color_count = 0;
            Color color_qingyise = 99;
            foreach (var color in m_valid_colors)
            {
                if (m[color] != 0)
                {
                    color_count += 1;
                    color_qingyise = color;
                }
            }
            return (color_count == 1, color_qingyise);
        }

        public bool is_daiyaojiu(long[] m)
        {
            var total_need = 0;

            /*foreach (var op_item in opgroup)
            {
                long color_eigen = op_item.opcards.Sum(card=>EIGEN_MAP[get_card_num(card)]);

                //牌包含4、5、6的牌
                //或包含8、7、3、2的刻子
                //则牌肯定不符合带幺九
                if( (color_eigen & 0x000FFF000) != 0 ||
                    (is_eigen_include(color_eigen, 0x3_030_000_000)) == true ||
                    (is_eigen_include(color_eigen, 0x3_003_000_000)) == true ||
                    (is_eigen_include(color_eigen, 0x3_000_000_300)) == true ||
                    (is_eigen_include(color_eigen, 0x3_000_000_030)) == true 
                    )
                {
                    return false;
                }
            }*/

            foreach (var color in m_valid_colors)
            {
                var color_eigen = m[color];
                if (color_eigen != 0)
                {
                    // 不能有4，5，6的值
                    if ((color_eigen & 0x000FFF000) != 0)
                    {
                        return false;
                    }
                }

                var count_of_1 = get_num_cardcount(color_eigen, 1);
                var count_of_2 = get_num_cardcount(color_eigen, 2);
                var count_of_3 = get_num_cardcount(color_eigen, 3);

                var shunzi_123_count = Math.Max(count_of_2, count_of_3);
                total_need = total_need + (shunzi_123_count - count_of_2);
                total_need = total_need + (shunzi_123_count - count_of_3);
                total_need = total_need + count_of_1 >= shunzi_123_count ? 0 : shunzi_123_count - count_of_1;

                var count_of_7 = get_num_cardcount(color_eigen, 7);
                var count_of_8 = get_num_cardcount(color_eigen, 8);
                var count_of_9 = get_num_cardcount(color_eigen, 9);

                var shunzi_789_count = Math.Max(count_of_7, count_of_8);
                total_need = total_need + (shunzi_789_count - count_of_7);
                total_need = total_need + (shunzi_789_count - count_of_8);
                total_need = total_need = count_of_9 >= shunzi_789_count ? 0 : shunzi_789_count - count_of_9;

                if (total_need > get_laizi_cardcount(m))
                {
                    return false;
                }
            }

            return true;
        }

        // 连七对
        public bool is_lianqidui(long[] m)
        {
            var total_cardcount = get_total_cardcount(m);
            if (total_cardcount != 14)
            {
                return false;
            }

            var (flag, color_qingyise) = is_qingyise(m);

            if (flag == false)
            {
                return false;
            }
            var color_eigen = m[color_qingyise];

            foreach (var lianqi_eigen in new long[] { 0xe222222200, 0xe022222220, 0xe002222222 })
            {
                if (is_eigen_include(lianqi_eigen, color_eigen))
                {
                    return true;
                }
            }
            return false;
        }

        public bool is_jiulianbaodeng(long[] m)
        {
            var total_cardcount = get_total_cardcount(m);
            if (total_cardcount != 14)
            {
                return false;
            }

            var (flag, color_qingyise) = is_qingyise(m);

            if (flag == false)
            {
                return false;
            }

            var color_eigen = m[color_qingyise];

            foreach (var jiulian_eigen in new long[]{
                0xe311111114,
                0xe311111123,
                0xe311111213,
                0xe311112113,
                0xe311121113,
                0xe311211113,
                0xe312111113,
                0xe321111113,
                0xe411111113,
            })
            {
                if (is_eigen_include(jiulian_eigen, color_eigen))
                {
                    return true;
                }
            }

            return false;
        }

        public List<Eigen> get_deigens(Color color, PairType pair_type)
        {
            return DEIGENS_MAP[color][(int)pair_type];
        }

        void rid_guzhang(Color color, Eigen eigen, List<List<(PairType, Eigen)>> tmp)
        {
            //var path_index = tmp.Count - 1;
            if (eigen != 0)
            {
                foreach (var num in Share.MahjongValue.VALID_NUMS[color])
                {
                    var count = get_num_cardcount(eigen, num);
                    if (count > 0)
                    {
                        var deigen = EIGEN_MAP[num];
                        eigen -= deigen;
                        for (int _ = 0; _ < count; _++)
                        {
                            var t = (PairType.Guzhang, deigen);
                            tmp[tmp.Count - 1].Add(t);
                        }
                    }
                }
            }

            tmp.Add(new List<(PairType, Eigen)>());
            return;
        }

        void rid_dazi(Color color, Eigen eigen, List<List<(PairType, Eigen)>> tmp)
        {
            var rid_times = 0;
            var a = get_deigens(color, PairType.DaZi);
            foreach (var deigen in a)
            {
                if (is_eigen_include(eigen, deigen))
                {
                    //var path_index = tmp.Count - 1;
                    var t = (PairType.DaZi, deigen);
                    tmp[tmp.Count - 1].Add(t);
                    eigen -= deigen;
                    rid_dazi(color, eigen, tmp);
                    eigen += deigen;
                    rid_times += 1;
                };

            }

            if (rid_times == 0)
            {
                rid_guzhang(color, eigen, tmp);
            }
            return;
        }

        void rid_mianzi(Color color, Eigen eigen, List<List<(PairType, Eigen)>> tmp)
        {
            var rid_times = 0;
            var a = get_deigens(color, PairType.MianZi);
            foreach (var deigen in a)
            {
                if (is_eigen_include(eigen, deigen))
                {
                    //var path_index = tmp.Count - 1;
                    var t = (PairType.MianZi, deigen);
                    tmp[tmp.Count - 1].Add(t);
                    eigen -= deigen;
                    rid_mianzi(color, eigen, tmp);
                    eigen += deigen;
                    rid_times += 1;
                }
            }

            if (rid_times == 0)
            {
                rid_dazi(color, eigen, tmp);
            }
            return;
        }


        public double get_tile_value(long[] m, Dictionary<Card, int> remain_map)
        {
            var tile_value = 1.0;

            Action<PairType, Cards, double> callback = (pair_type, _cards, weight_total) =>
            {
                tile_value *= weight_total;
            };

            split_tile(m, remain_map, callback);
            return tile_value;
        }


        public List<(Card card, double weight)> get_outcard_suggestion(long[] m, Dictionary<Card, int> remain_map)
        {
            Dictionary<Card, double> ting_cards_info = new Dictionary<Card, double>();

            Action<PairType, Cards, double> callback = (pair_type, cards, weight_total) =>
            {
                foreach (var card in cards)
                {
                    var weight = weight_total;
                    ting_cards_info.TryGetValue(card, out weight);

                    if (pair_type == PairType.Guzhang)
                    {
                    }
                    else
                    {
                        weight *= 1.2;
                    }
                    ting_cards_info[card] = weight;

                }
            };
            split_tile(m, remain_map, callback);

            var a = new List<(Card card, double weight)>();

            foreach (var item in ting_cards_info)
            {
                a.Add((item.Key, item.Value));
            }
            return a;
        }

        //weight是牌的价值
        public void split_tile(long[] m,
                Dictionary<Card, int> remain_map,
                Action<PairType, Cards, double> callback
            )
        {
            var total_cardcount = get_total_cardcount(m);

            foreach (var color in m_valid_colors)
            {
                var color_eigen = m[color];
                if (color_eigen == 0)
                {
                    continue;
                }

                var eigen_cardcount = get_eigen_cardcount(color_eigen);
                List<List<(PairType, Eigen)>> tmp = new List<List<(PairType, Eigen)>>() { };
                tmp.Add(new List<(PairType, Eigen)>());
                rid_mianzi(color, color_eigen, tmp);

                foreach (var v in tmp)
                {
                    foreach (var vv in v)
                    {
                        var pair_type = vv.Item1;
                        var deigen = vv.Item2;

                        Cards cards = new Cards();
                        // todo 优化为直接取预存结果
                        foreach (var num in Share.MahjongValue.VALID_NUMS[color])
                        {
                            var count = get_num_cardcount(deigen, num);
                            if (count > 0)
                            {
                                var card = make_card(color, num);
                                cards.Add(card);
                            }
                        }

                        // 牌数越少的花色权重越低
                        double weight_total = 1.0;
                        double weight_cardcount = (double)eigen_cardcount / total_cardcount;
                        weight_total *= weight_cardcount;

                        // 连续程度越少的牌权重越低（孤张最低）
                        double weight_pairtype = 1;
                        if (pair_type == PairType.MianZi) { weight_pairtype = MIANZI_WEIGHT_MAP[deigen]; }
                        if (pair_type == PairType.DaZi) { weight_pairtype = DAZI_WEIGHT_MAP[deigen]; }
                        if (pair_type == PairType.Guzhang) { weight_pairtype = 0.4; }

                        weight_total *= weight_pairtype;

                        // 凑的牌的剩余数量越少的牌权重越低
                        Cards lack_cards = new Cards();

                        if (pair_type == PairType.DaZi)
                        {
                            lack_cards = DAZI_TING_NUM_MAP[deigen].ConvertAll(num => make_card(color, num));
                        }
                        else if (pair_type == PairType.Guzhang)
                        {
                            lack_cards = cards.ToList();
                        }


                        if (lack_cards.Count > 0)
                        {
                            double random_count = 6;
                            double total_count = lack_cards.ConvertAll(v => remain_map[v]).Sum();

                            double weight_remaincount = total_count / random_count;
                            if (weight_remaincount > 0.0)
                            {
                                weight_total *= weight_remaincount;
                            }
                        }

                        callback(pair_type, cards, weight_total);
                    }
                }

            }
        }
    }
}
