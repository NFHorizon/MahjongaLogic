#![feature(test)]
extern crate test;
use test::bench::Bencher;

// #[cfg(test)]

use std::cmp;
use std::collections::HashMap;
use std::collections::HashSet;
use std::fs::File;
// use std::error::Error;
// use std::collections::BTreeSet;
use std::hash::BuildHasherDefault;
use std::io::prelude::*;
// use std::option::Option;
use std::path::Path;

use fnv::FnvHasher;
use lazy_static::lazy_static;
use rand::Rng;

type Eigen = u64;
/*Matrix形如
    [
        [0]:0x00_000_000_000,
        [1]:0x00_000_000_000,
        [2]:0x00_000_000_000,
        [3]:0x00_000_000_000,
        [4]:0x00_000_000_000,
        [5]:0x00_000_000_000,
        [6]:0x00_000_000_000,
        [7]:total_card_count //牌总数
        [8]:laizi_count //癞子数
    ]
    Cards形如{
*/
type Matrix = [Eigen; 9];
type Color = usize;
type Num = usize;
type Card = u8;
type Cards = Vec<Card>;

const INVALID_CARD: Card = 99;
const EIGEN_MAP: [Eigen; 10] = [
    0,
    0x1_000_000_001,
    0x1_000_000_010,
    0x1_000_000_100,
    0x1_000_001_000,
    0x1_000_010_000,
    0x1_000_100_000,
    0x1_001_000_000,
    0x1_010_000_000,
    0x1_100_000_000,
];

const SHIFT_MAP: [u32; 11] = [
    0, 0, //1
    4, //2
    8, 12, 16, 20, 24, 28, 32, //9
    36,
];
const KEZI_ARR: [Eigen; 10] = [
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
];
const SHUNZI_ARR: [Eigen; 8] = [
    0,
    0x3_000_000_111,
    0x3_000_001_110,
    0x3_000_011_100,
    0x3_000_111_000,
    0x3_001_110_000,
    0x3_011_100_000,
    0x3_111_000_000,
];
const EYES_ARR: [Eigen; 10] = [
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
];

const DAZI_ARR: [Eigen; 24] = [
    0x2000000011,
    0x2110000000, // 边张
    0x2000000110,
    0x2000001100,
    0x2000011000,
    0x2000110000,
    0x2001100000,
    0x2011000000, // 两面
    0x2000000101,
    0x2000001010,
    0x2000010100,
    0x2000101000,
    0x2001010000,
    0x2010100000,
    0x2101000000, // 嵌张
    0x2000000002,
    0x2000000020,
    0x2000000200,
    0x2000002000,
    0x2000020000,
    0x2000200000,
    0x2002000000,
    0x2020000000,
    0x2200000000, // 对子
];

// 风牌不能组成顺子
const FENG_MIANZI_ARR: [Eigen; 4] = [
    0x3_000_000_003,
    0x3_000_000_030,
    0x3_000_000_300,
    0x3_000_003_000,
];

const FENG_DAZI_ARR: [Eigen; 4] = [0x2000000002, 0x2000000020, 0x2000000200, 0x2000002000];

const JIAN_MIANZI_ARR: [Eigen; 3] = [0x3_000_000_003, 0x3_000_000_030, 0x3_000_000_300];

const JIAN_DAZI_ARR: [Eigen; 3] = [0x2000000002, 0x2000000020, 0x2000000200];

const MIANZI_ARR: [Eigen; 16] = [
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
];

const INVALID_COLOR: Color = 99;
const COLOR_WAN: Color = 0;
const COLOR_TONG: Color = 1;
const COLOR_TIAO: Color = 2;
const COLOR_FENG: Color = 3;
const COLOR_JIAN: Color = 4;
const COLOR_HUA: Color = 5;
const COLOR_JOKER: Color = 6;
const INDEX_CARD_COUNT: Color = 7;
const INDEX_LAIZI_COUNT: Color = 8;

const VALID_NUMS: [&[Num]; 7] = [
    &[1, 2, 3, 4, 5, 6, 7, 8, 9], //[COLOR_WAN]
    &[1, 2, 3, 4, 5, 6, 7, 8, 9], //[COLOR_TONG]
    &[1, 2, 3, 4, 5, 6, 7, 8, 9], //[COLOR_TIAO]
    &[1, 2, 3, 4],                //[COLOR_FENG]
    &[1, 2, 3],                   //[COLOR_JIAN]
    &[1, 2, 3, 4, 5, 6, 7, 8],    //[COLOR_HUA]
    &[1],                         //[COLOR_JOKER]
];

// type HuEigenSet<K> = BTreeSet<K>;
type HuEigenMap = HashMap<Eigen, i32, BuildHasherDefault<FnvHasher>>;

pub enum HumapType {
    BASE,
    FENG,
}

enum PairType {
    MianZi,
    DaZi,
    Guzhang,
}

struct TingPair {
    is_ting_any: bool,
    ting_cards: Vec<Card>,
}
struct Deigen {
    deigen: Eigen,
    pair_type: PairType,
}

lazy_static! {

    //14张牌情况下，正确大小为21743（含0）；17张牌情况下，正确大小为53055（含0）
    // static ref BASE_HUMAP: HuEigenSet = gen_humap(&HumapType::BASE);
    //14张情况下，正确大小为48（含0）；17张牌情况下，正确大小为48（含0）
    // static ref FENG_HUMAP: HuEigenSet = gen_humap(&HumapType::FENG);

    //14张情况下，正确大小为227980（含0）；17张牌情况下，正确大小为627091（含0）
    static ref LAIZI_HUMAP: HuEigenMap = gen_laizimap(&HumapType::BASE, 7);
    //14张情况下，正确大小为237（含0）；17张牌情况下，正确大小为237（含0）
    // static ref FENG_LAIZI_HUMAP: HuEigenSet = gen_laizimap(&HumapType::FENG, 4);

    static ref REVERSE_EIGEN_MAP: HashMap<Eigen, usize> = {
       let mut m = HashMap::new();
        m.insert(0x0000000000, 0);
        m.insert(0x1000000001, 1);
        m.insert(0x1000000010, 2);
        m.insert(0x1000000100, 3);
        m.insert(0x1000001000, 4);
        m.insert(0x1000010000, 5);
        m.insert(0x1000100000, 6);
        m.insert(0x1001000000, 7);
        m.insert(0x1010000000, 8);
        m.insert(0x1100000000, 9);
        m
    };

    static ref DAZI_WEIGHT_MAP: HashMap<Eigen, f32> = {
        let mut m = HashMap::new();
        m.insert(0x2000000011, 0.7);
        m.insert(0x2110000000, 0.7);
        m.insert(0x2000000110, 0.8);
        m.insert(0x2000001100, 0.8);
        m.insert(0x2000011000, 0.8);
        m.insert(0x2000110000, 0.8);
        m.insert(0x2001100000, 0.8);
        m.insert(0x2011000000, 0.8);
        m.insert(0x2000000101, 0.7);
        m.insert(0x2000001010, 0.7);
        m.insert(0x2000010100, 0.7);
        m.insert(0x2000101000, 0.7);
        m.insert(0x2001010000, 0.7);
        m.insert(0x2010100000, 0.7);
        m.insert(0x2101000000, 0.7);
        m.insert(0x2000000002, 0.9);
        m.insert(0x2000000020, 0.9);
        m.insert(0x2000000200, 0.9);
        m.insert(0x2000002000, 0.9);
        m.insert(0x2000020000, 0.9);
        m.insert(0x2000200000, 0.9);
        m.insert(0x2002000000, 0.9);
        m.insert(0x2020000000, 0.9);
        m.insert(0x2200000000, 0.9);
        m
    };

    static ref DAZI_TING_NUM_MAP: HashMap<Eigen, Vec<Num>> = {
        let mut m = HashMap::new();
        m.insert(0x1000000011, vec![3]);
        m.insert(0x1110000000, vec![7]);
        m.insert(0x2000000110, vec![1, 4]);
        m.insert(0x2000001100, vec![2, 5]);
        m.insert(0x2000011000, vec![3, 6]);
        m.insert(0x2000110000, vec![4, 7]);
        m.insert(0x2001100000, vec![5, 8]);
        m.insert(0x2011000000, vec![6, 9]);
        m.insert(0x1000000101, vec![2]);
        m.insert(0x1000001010, vec![3]);
        m.insert(0x1000010100, vec![4]);
        m.insert(0x1000101000, vec![5]);
        m.insert(0x1001010000, vec![6]);
        m.insert(0x1010100000, vec![7]);
        m.insert(0x1101000000, vec![8]);
        m.insert(0x1000000002, vec![1]);
        m.insert(0x1000000020, vec![2]);
        m.insert(0x1000000200, vec![3]);
        m.insert(0x1000002000, vec![4]);
        m.insert(0x1000020000, vec![5]);
        m.insert(0x1000200000, vec![6]);
        m.insert(0x1002000000, vec![7]);
        m.insert(0x1020000000, vec![8]);
        m.insert(0x1200000000, vec![9]);
        m
    };

    static ref MIANZI_WEIGHT_MAP: HashMap<Eigen, f32> = {
        let mut m = HashMap::new();
        m.insert(0x3000000003, 1.1);
        m.insert(0x3000000030, 1.1);
        m.insert(0x3000000300, 1.1);
        m.insert(0x3000003000, 1.1);
        m.insert(0x3000030000, 1.1);
        m.insert(0x3000300000, 1.1);
        m.insert(0x3003000000, 1.1);
        m.insert(0x3030000000, 1.1);
        m.insert(0x3300000000, 1.1);
        m.insert(0x3000000111, 1.1);
        m.insert(0x3000001110, 1.1);
        m.insert(0x3000011100, 1.1);
        m.insert(0x3000111000, 1.1);
        m.insert(0x3001110000, 1.1);
        m.insert(0x3011100000, 1.1);
        m.insert(0x3111000000, 1.1);
        m
    };
}

fn combine(
    data: &Vec<Eigen>,
    step: usize,
    select_data: &mut Vec<Eigen>,
    target_num: usize,
    callback: &mut dyn FnMut(Eigen),
) {
    if select_data.len() == target_num {
        //选择的元素已经够了，就输出并返回
        callback(select_data.iter().sum());
        return;
    }

    if step >= data.len() {
        //没有元素选了而且还没够，也是直接返回
        return;
    }
    select_data.push(data[step]); //选择当前元素
    combine(data, step + 1, select_data, target_num, callback);
    select_data.pop(); //别忘了从已选择元素中先删除
    combine(data, step + 1, select_data, target_num, callback); //不选择当前元素
}

fn gen_humap(humap_type: &HumapType) -> HuEigenMap {
    let max_handcard_count = 17;
    let mut m = HuEigenMap::default();

    let max_num = match humap_type {
        HumapType::BASE => 9,
        HumapType::FENG => 4,
    };

    for mianzi_cnt in 1..=(max_handcard_count - 2) / 3 {
        let mut data = vec![0]; //0这里代表没有面子的情况
        for num in 1..=max_num {
            data.push(KEZI_ARR[num]);
        }

        if let HumapType::BASE = humap_type {
            for _ in 0..mianzi_cnt {
                for num in 1..=7 {
                    data.push(SHUNZI_ARR[num]);
                }
            }
        };

        let mut callback = |mut select_data_eigen: Eigen| {
            if !m.contains_key(&select_data_eigen) {
                for &v in &EYES_ARR[0..(max_num + 1)] {
                    select_data_eigen += v;
                    if is_valid_cardtype(select_data_eigen) {
                        m.insert(select_data_eigen, 0);
                        //插入结果表
                    }
                    select_data_eigen -= v;
                }
            }
        };

        combine(&data, 0, &mut vec![], mianzi_cnt, &mut callback);
    }

    return m;
}

fn gen_laizimap(humap_type: &HumapType, target_que_count: i32) -> HuEigenMap {
    let max_num = match humap_type {
        HumapType::BASE => 9,
        HumapType::FENG => 4,
    };

    let mut humap = gen_humap(&humap_type);
    let mut m = gen_humap(&humap_type);

    let mut gen_sub_map = |humap: &HuEigenMap, que_count| {
        let mut new_m = HuEigenMap::default();
        for (&eigen, &old_que_count) in humap {
            if old_que_count + 1 > que_count {
                continue;
            }
            for num in 1..=max_num {
                if MahjongLogic::get_num_cardcount(eigen, num) >= 1 {
                    let new_eigen = eigen - EIGEN_MAP[num];
                    if new_eigen == 0 {
                        continue;
                    }

                    let value = m.get(&new_eigen);
                    match value {
                        Some(&x) => {
                            if x > old_que_count + 1 {
                                m.insert(new_eigen, old_que_count + 1);
                                new_m.insert(new_eigen, old_que_count + 1);
                            }
                        }
                        None => {
                            m.insert(new_eigen, old_que_count + 1);
                            new_m.insert(new_eigen, old_que_count + 1);
                        }
                    }
                }
            }
        }
        return new_m;
    };

    let mut que_count = 1;
    while que_count <= target_que_count {
        humap = gen_sub_map(&humap, que_count);
        que_count += 1;
    }

    return m;
}

fn is_valid_cardtype(eigen: Eigen) -> bool {
    for num in 1..=9 {
        if MahjongLogic::get_num_cardcount(eigen, num) > 4 {
            return false;
        }
    }
    return true;
}

fn _shuffle(mut card_walls: Cards) -> Cards {
    let mut rng = rand::thread_rng();

    //rust默认shuffle
    //    rng.shuffle(wall.as_mut_slice());
    //Knuth 洗牌算法
    for i in card_walls.len() - 1..=0 {
        card_walls.swap(i, rng.gen_range(0..i));
    }
    return card_walls;
}

fn _print_hutable(humap: &HuEigenMap, path: &Path) {
    println!("print_hutable");

    let mut hutable_str = "".to_string();
    for (&eigen, &quecount) in humap {
        hutable_str += &format!("   [{:X}] = {},\n", eigen, quecount);
    }
    let display = path.display();

    // 以只写模式打开文件，返回 `io::Result<File>`
    let mut file = match File::create(&path) {
        Err(why) => panic!("couldn't create {}: {}", display, why.to_string()),
        Ok(file) => file,
    };
    // 将 `LOREM_IPSUM` 字符串写进 `file`，返回 `io::Result<()>`
    match file.write_all(hutable_str.as_bytes()) {
        Err(why) => panic!("couldn't write to {}: {}", display, why.to_string()),
        Ok(_) => println!("successfully wrote to {}", display),
    }
}

fn _get_new_random_card_walls() -> Cards {
    let card_walls = vec![
        0x01, 0x01, 0x01, 0x01, 0x02, 0x02, 0x02, 0x02, 0x03, 0x03, 0x03, 0x03, 0x04, 0x04, 0x04,
        0x04, 0x05, 0x05, 0x05, 0x05, 0x06, 0x06, 0x06, 0x06, 0x07, 0x07, 0x07, 0x07, 0x08, 0x08,
        0x08, 0x08, 0x09, 0x09, 0x09, 0x09, 0x11, 0x11, 0x11, 0x11, 0x12, 0x12, 0x12, 0x12, 0x13,
        0x13, 0x13, 0x13, 0x14, 0x14, 0x14, 0x14, 0x15, 0x15, 0x15, 0x15, 0x16, 0x16, 0x16, 0x16,
        0x17, 0x17, 0x17, 0x17, 0x18, 0x18, 0x18, 0x18, 0x19, 0x19, 0x19, 0x19, 0x21, 0x21, 0x21,
        0x21, 0x22, 0x22, 0x22, 0x22, 0x23, 0x23, 0x23, 0x23, 0x24, 0x24, 0x24, 0x24, 0x25, 0x25,
        0x25, 0x25, 0x26, 0x26, 0x26, 0x26, 0x27, 0x27, 0x27, 0x27, 0x28, 0x28, 0x28, 0x28, 0x29,
        0x29, 0x29, 0x29, 0x31, 0x31, 0x31, 0x31, 0x32, 0x32, 0x32, 0x32, 0x33, 0x33, 0x33, 0x33,
        0x34, 0x34, 0x34, 0x34, 0x41, 0x41, 0x41, 0x41, 0x42, 0x42, 0x42, 0x42, 0x43, 0x43, 0x43,
        0x43,
    ];

    _shuffle(card_walls)
}

struct MahjongLogic {
    valid_colors: Vec<Color>,
    laizi_cards_set: HashSet<Card>,
    contain_qidui: bool,
}

impl MahjongLogic {
    //获取牌数字

    pub extern "C" fn get_card_num(card: Card) -> Num {
        (card & 0x0F) as Num
    }

    //获取牌的花色
    pub extern "C" fn get_card_color(card: Card) -> Color {
        (card >> 4) as Color
    }

    //用花色和牌数字还原牌
    pub extern "C" fn make_card(color: Color, num: Num) -> Card {
        ((color << 4) + num) as Card
    }

    //获取num对应的eigen
    fn get_num_eigen(num: Num) -> Eigen {
        return EIGEN_MAP[num];
    }

    //获取矩阵手牌牌的总数
    fn get_total_cardcount(m: Matrix) -> i32 {
        return m[INDEX_CARD_COUNT] as i32;
    }

    fn get_laizi_cardcount(m: Matrix) -> i32 {
        return m[INDEX_LAIZI_COUNT] as i32;
    }

    //获取特征值手牌牌的总数
    fn get_eigen_cardcount(eigen: Eigen) -> i32 {
        return (eigen >> 36) as i32;
    }

    //取特征值某一位的值（表示牌的数量）
    fn get_num_cardcount(eigen: Eigen, num: Num) -> i32 {
        return ((eigen >> SHIFT_MAP[num]) & 0xF) as i32;
    }

    fn is_eigen_include(eigen: Eigen, deigen: Eigen) -> bool {
        return if eigen < deigen {
            false
        } else {
            ((eigen - deigen) & 0x888_888_888) == 0
        };
    }

    fn is_eigen_match(color_eigen: Eigen) -> i32 {
        return LAIZI_HUMAP.get(&color_eigen).cloned().unwrap_or(99);
    }
}

impl MahjongLogic {
    //往牌矩阵里添加一张牌
    #[no_mangle]
    pub extern "C" fn matrix_add_card(&self, m: &mut Matrix, card: Card) {
        let num = MahjongLogic::get_card_num(card);
        let color = MahjongLogic::get_card_color(card);
        m[color] += MahjongLogic::get_num_eigen(num);
        if self.is_laizi(card) {
            m[INDEX_LAIZI_COUNT] += 1;
        }
        m[INDEX_CARD_COUNT] += 1;
    }

    #[no_mangle]
    pub extern "C" fn matrix_remove_card(&self, m: &mut Matrix, card: Card) {
        let num = MahjongLogic::get_card_num(card);
        let color = MahjongLogic::get_card_color(card);

        if !MahjongLogic::is_eigen_include(m[color], EIGEN_MAP[num]) {
            println!("手牌矩阵中不包含该牌");
            return;
        }

        m[color] -= MahjongLogic::get_num_eigen(num);
        if self.is_laizi(card) {
            m[INDEX_LAIZI_COUNT] -= 1;
        }
        m[INDEX_CARD_COUNT] -= 1;
    }

    //往牌矩阵里添加牌向量
    #[no_mangle]
    pub extern "C" fn matrix_add_cards(&self, m: &mut Matrix, cards: Cards) {
        for &card in &cards {
            self.matrix_add_card(m, card);
        }
    }

    //将原始手牌数组变成二维数组
    #[no_mangle]
    pub extern "C" fn cards_to_matrix(&self, cards: Cards) -> Matrix {
        let mut m: Matrix = [0, 0, 0, 0, 0, 0, 0, 0, 0];
        self.matrix_add_cards(&mut m, cards);
        return m;
    }

    //二维数组还原为原始手牌数组
    #[no_mangle]
    pub extern "C" fn matrix_to_cards(&self, m: Matrix) -> Cards {
        let mut cards = Cards::new();

        for &color in &self.valid_colors {
            for &num in VALID_NUMS[color] {
                let count = MahjongLogic::get_num_cardcount(m[color], num);
                if count > 0 {
                    let card = MahjongLogic::make_card(color, num);
                    for _i in 1..=count {
                        cards.push(card);
                    }
                }
            }
        }

        return cards;
    }

    //检查两牌矩阵是否相等
    #[no_mangle]
    pub extern "C" fn matrix_equal(&self, m1: Matrix, m2: Matrix) -> bool {
        //先比较牌的数量
        if m1[INDEX_CARD_COUNT] != m2[INDEX_CARD_COUNT] {
            return false;
        }
        //逐个比较各个花色
        for &color in &self.valid_colors {
            if m1[color] != m2[color] {
                return false;
            }
        }
        return true;
    }

    fn is_laizi(&self, card: Card) -> bool {
        return self.laizi_cards_set.contains(&card);
    }

    #[no_mangle]
    pub extern "C" fn shelter_laizi(&self, &m: &Matrix) -> Matrix {
        if self.laizi_cards_set.len() == 0 {
            return m;
        }
        let mut new_m = m.clone();
        for &card in &self.laizi_cards_set {
            let color = MahjongLogic::get_card_color(card);
            let num = MahjongLogic::get_card_num(card);
            let count = MahjongLogic::get_num_cardcount(m[color], num) as u64;
            new_m[color] -= EIGEN_MAP[num] * count
        }
        return new_m;
    }

    #[no_mangle]
    pub extern "C" fn get_gen(&self, m: Matrix) -> Cards {
        let mut cards: Cards = Cards::default();
        for &color in &self.valid_colors {
            if m[color] & 0x444_444_444 != 0 {
                for &num in VALID_NUMS[color] {
                    if MahjongLogic::get_num_cardcount(m[color], num) == 4 {
                        cards.push(MahjongLogic::make_card(color, num));
                    }
                }
            }
        }
        return cards;
    }

    #[no_mangle]
    pub extern "C" fn get_kezi(&self, m: Matrix, contain_gen: bool) -> Cards {
        let mut cards: Cards = Cards::default();
        for &color in &self.valid_colors {
            for &num in VALID_NUMS[color] {
                let count = MahjongLogic::get_num_cardcount(m[color], num);
                if count == 3 || (contain_gen && count == 4) {
                    cards.push(MahjongLogic::make_card(color, num));
                }
            }
        }
        return cards;
    }

    //获取“根”的数量 （根即手牌有4张）
    #[no_mangle]
    pub extern "C" fn get_gen_count(&self, m: Matrix) -> u32 {
        let mut gen_count = 0;
        for &color in &self.valid_colors {
            let color_eigen = m[color];
            gen_count += (color_eigen & 0x444_444_444).count_ones();
        }

        gen_count
    }

    #[no_mangle]
    pub extern "C" fn get_ting_pairs(&self, mut m: Matrix) -> HashMap<Card, TingPair> {
        let mut ting_pairs: HashMap<Card, TingPair> = HashMap::new();
        for &color in &self.valid_colors {
            let color_eigen = m[color];
            if color_eigen == 0 {
                continue;
            }

            for &num in VALID_NUMS[color] {
                let count = MahjongLogic::get_num_cardcount(color_eigen, num);
                if count > 0 {
                    let card = MahjongLogic::make_card(color, num);
                    self.matrix_remove_card(&mut m, card);
                    if self.is_ting_any(m) {
                        ting_pairs.insert(
                            card,
                            TingPair {
                                is_ting_any: true,
                                ting_cards: vec![],
                            },
                        );
                    } else {
                        let ting_cards = self.get_ting_cards(m);
                        if ting_cards.len() > 0 {
                            ting_pairs.insert(
                                card,
                                TingPair {
                                    is_ting_any: false,
                                    ting_cards: ting_cards,
                                },
                            );
                        }
                    }
                    self.matrix_add_card(&mut m, card);
                }
            }
        }

        return ting_pairs;
    }

    #[no_mangle]
    pub extern "C" fn get_ting_cards(&self, m: Matrix) -> Cards {
        let mut ting_cards = vec![];
        if (MahjongLogic::get_total_cardcount(m) + 1) % 3 != 2 {
            return ting_cards;
        } //要嘛加上一张牌可以凑成3N牌型（不带将）要嘛凑成3N+2（包含七对的14张）

        if self.contain_qidui {
            let qidui_ting_card = self.get_ting_qidui_card(m);
            if qidui_ting_card != INVALID_CARD {
                ting_cards.push(qidui_ting_card);
            }
        }

        //各种牌型的次数
        let mut count_2 = 0; //将
        let mut ting_colors = vec![];
        let mut total_need = 0;

        //满足3N+2的在这里计算
        for &color in &self.valid_colors {
            let color_eigen = m[color];
            //跳过没有牌的花色
            if color_eigen == 0 {
                continue;
            }
            let color_cardcount = MahjongLogic::get_eigen_cardcount(color_eigen);
            let color_need = MahjongLogic::is_eigen_match(color_eigen);

            if color_need == 99 {
                return ting_cards;
            }

            total_need += color_need;
            let total_lack = total_need - MahjongLogic::get_laizi_cardcount(m);
            if total_lack - 1 > 0 {
                return ting_cards;
            }

            if (color_cardcount + color_need) % 3 == 2 {
                count_2 += 1;
            }

            if total_lack - 1 > 1 - count_2 {
                return ting_cards;
            }

            ting_colors.push(color);
        }

        //逐个试
        for color in ting_colors {
            let color_eigen = m[color];

            let color_cardcount = MahjongLogic::get_eigen_cardcount(m[color]);
            let mut borrow_count_2 = 0;

            let color_need = MahjongLogic::is_eigen_match(color_eigen);

            if (color_cardcount + color_need) % 3 == 2 {
                borrow_count_2 = 1;
                count_2 -= borrow_count_2;
            }

            for &num in VALID_NUMS[color] {
                //添加完牌之后的新特征值
                let new_eigen = m[color] + EIGEN_MAP[num];
                let new_cardcount = color_cardcount + 1;

                let new_color_need = MahjongLogic::is_eigen_match(new_eigen);
                let new_total_need = total_need - color_need + new_color_need;

                if new_color_need == 99 {
                    continue;
                }

                let total_lack = new_total_need - MahjongLogic::get_laizi_cardcount(m);
                if total_lack > 0 {
                    continue;
                }

                let new_count_2 = count_2 + ((new_cardcount + new_color_need) % 3) / 2;
                if total_lack > 1 - new_count_2 {
                    continue;
                }

                ting_cards.push(MahjongLogic::make_card(color, num));
            }

            count_2 += borrow_count_2;
        }

        return ting_cards;
    }

    fn match_shape(&self, &m: &Matrix, max_lack: i32) -> bool {
        //各种牌型的次数
        let mut count_2 = 0; //将
        let mut total_need = 0; //需要的癞子数量

        //满足3N+2的在这里计算
        for &color in &self.valid_colors {
            let color_eigen = m[color];
            //跳过没有牌的花色
            if color_eigen == 0 {
                continue;
            }

            let color_cardcount = MahjongLogic::get_eigen_cardcount(color_eigen);
            let color_need = MahjongLogic::is_eigen_match(color_eigen);

            if color_need == 99 {
                return false;
            }

            total_need += color_need;

            let total_lack = total_need - MahjongLogic::get_laizi_cardcount(m);

            if total_lack > max_lack {
                return false;
            }

            //计算将个数
            if (color_cardcount + color_need) % 3 == 2 {
                count_2 += 1;
            }

            if total_lack > max_lack + 1 - count_2 {
                return false;
            }
        }
        //满足胡牌队列
        return true;
    }

    #[no_mangle]
    pub extern "C" fn is_ting(&self, m: Matrix) -> bool {
        if MahjongLogic::get_total_cardcount(m) % 3 != 1 {
            return false;
        }
        // 七对判断
        if self.contain_qidui && self.is_ting_qidui(m) {
            return true;
        }

        self.match_shape(&m, 1)
    }

    #[no_mangle]
    pub extern "C" fn is_ting_any(&self, mut m: Matrix) -> bool {
        if m[INDEX_LAIZI_COUNT]==0 {
            return false;
        }
        m[INDEX_LAIZI_COUNT] -= 1;
        m[INDEX_CARD_COUNT] -= 1;
        let res = self.is_complete(m);
        m[INDEX_LAIZI_COUNT] += 1;
        m[INDEX_CARD_COUNT] += 1;
        return res;
    }

    #[no_mangle]
    pub extern "C" fn is_complete(&self, m: Matrix) -> bool {
        if MahjongLogic::get_total_cardcount(m) % 3 != 1 {
            return false;
        }
        return self.match_shape(&m, 0);
    }

    #[no_mangle]
    pub extern "C" fn is_hu(&self, m: Matrix) -> bool {
        if MahjongLogic::get_total_cardcount(m) % 3 != 2 {
            return false;
        }

        // 七对判断
        if self.contain_qidui && self.belong_qidui(m) {
            return true;
        }

        return self.match_shape(&m, 0);
    }

    #[no_mangle]
    pub extern "C" fn test_hu(&self, mut m: Matrix, card: Card) -> bool {
        self.matrix_add_card(&mut m, card);
        let res = self.is_hu(m);
        self.matrix_remove_card(&mut m, card);
        return res;
    }

    fn match_qidui_shape(&self, &m: &Matrix, max_lack: i32) -> bool {
        let mut total_need = 0;

        for &color in &self.valid_colors {
            for &num in VALID_NUMS[color] {
                total_need += MahjongLogic::get_num_cardcount(m[color], num) & 0x1;
            }
        }

        if total_need - MahjongLogic::get_laizi_cardcount(m) > max_lack {
            return false;
        }

        return true;
    }

    fn is_ting_qidui(&self, m: Matrix) -> bool {
        let total_cardcount = MahjongLogic::get_total_cardcount(m);
        if total_cardcount != 13 {
            return false;
        }

        return self.match_qidui_shape(&m, 1);
    }

    //属于七对
    #[no_mangle]
    pub fn belong_qidui(&self, m: Matrix) -> bool {
        let total_cardcount = MahjongLogic::get_total_cardcount(m);
        if total_cardcount != 14 {
            return false;
        }

        if m[INDEX_LAIZI_COUNT] == 0 {
            for &color in &self.valid_colors {
                let color_eigen = m[color];
                if color_eigen & 0x111_111_111 != 0 {
                    return false;
                }
            }
        } else {
            return self.match_qidui_shape(&m, 0);
        }

        return true;
    }

    //判断是否听七对
    fn get_ting_qidui_card(&self, m: Matrix) -> Card {
        let total_cardcount = MahjongLogic::get_total_cardcount(m);
        if total_cardcount != 13 {
            return INVALID_CARD;
        }

        if m[INDEX_LAIZI_COUNT] == 0 {
            let mut possible_qidui_num = 0;
            let mut possible_qidui_color = INVALID_COLOR;

            for &color in &self.valid_colors {
                let color_eigen = m[color];
                if color_eigen == 0 {
                    continue;
                }

                let bitand_res = color_eigen & 0x111_111_111;
                if bitand_res != 0 {
                    //有重复的潜在听七对的数字，本牌型肯定不听七对
                    if possible_qidui_color != INVALID_COLOR {
                        return INVALID_CARD;
                    }

                    if bitand_res & (bitand_res - 1) == 0 {
                        let key = bitand_res + 0x1000_000_000;
                        match REVERSE_EIGEN_MAP.get(&key) {
                            Some(&x) => {
                                possible_qidui_num = x;
                                possible_qidui_color = color
                            }
                            None => {
                                // possible_qidui_num = 0;
                                panic!("计算错误，EIGEN_MAP不包含bitand_res {}", bitand_res);
                            }
                        }
                    } else {
                        return INVALID_CARD;
                    }
                }
            }

            if possible_qidui_color != INVALID_COLOR {
                return MahjongLogic::make_card(possible_qidui_color, possible_qidui_num);
            }
        }

        return INVALID_CARD;
    }

    //判断是否属于十三幺
    #[no_mangle]
    fn belong_shisanyao(&self, m: Matrix) -> bool {
        let total_cardcount = MahjongLogic::get_total_cardcount(m);
        if total_cardcount != 14 {
            return false;
        }

        for &color in &self.valid_colors {
            let color_eigen = m[color];

            let match_eigen = match color {
                COLOR_WAN | COLOR_TONG | COLOR_TIAO => 0x100_000_001,
                COLOR_FENG => 0x1111,
                COLOR_JIAN => 0x111,
                _ => 0,
            };

            if !MahjongLogic::is_eigen_include(color_eigen, match_eigen) {
                return false;
            }
        }
        return true;
    }

    fn get_ting_shisanyao_card(m: Matrix) -> Card {
        return INVALID_CARD;
    }

    #[no_mangle]
    pub extern "C" fn is_qingyise(&self, m: Matrix) -> Color {
        let mut color_count = 0;
        let mut color_qingyise: Color = 99;
        for &color in &self.valid_colors {
            if m[color] != 0 {
                color_count += 1;
                color_qingyise = color;
            }
        }
        return color_qingyise;
    }

    #[no_mangle]
    pub extern "C" fn is_daiyaojiu(&self, m: Matrix) -> bool {
        let mut total_need = 0;

        for &color in &self.valid_colors {
            let color_eigen = m[color];
            if color_eigen != 0 {
                // 不能有4，5，6的值
                if color_eigen & 0x000FFF000 != 0 {
                    return false;
                }
            }

            let count_of_1 = MahjongLogic::get_num_cardcount(color_eigen, 1);
            let count_of_2 = MahjongLogic::get_num_cardcount(color_eigen, 2);
            let count_of_3 = MahjongLogic::get_num_cardcount(color_eigen, 3);

            let shunzi_123_count = cmp::max(count_of_2, count_of_3);
            total_need = total_need + (shunzi_123_count - count_of_2);
            total_need = total_need + (shunzi_123_count - count_of_3);
            total_need = total_need
                + (if count_of_1 >= shunzi_123_count {
                    0
                } else {
                    shunzi_123_count - count_of_1
                });

            let count_of_7 = MahjongLogic::get_num_cardcount(color_eigen, 7);
            let count_of_8 = MahjongLogic::get_num_cardcount(color_eigen, 8);
            let count_of_9 = MahjongLogic::get_num_cardcount(color_eigen, 9);

            let shunzi_789_count = cmp::max(count_of_7, count_of_8);
            total_need = total_need + (shunzi_789_count - count_of_7);
            total_need = total_need + (shunzi_789_count - count_of_8);
            total_need = total_need
                + (if count_of_9 >= shunzi_789_count {
                    0
                } else {
                    shunzi_789_count - count_of_9
                });

            if total_need > MahjongLogic::get_laizi_cardcount(m) {
                return false;
            }
        }

        return true;
    }

    // 连七对
    #[no_mangle]
    pub extern "C" fn is_lianqidui(&self, m: Matrix) -> bool {
        let color_qingyise = self.is_qingyise(m);

        if color_qingyise == 99 {
            return false;
        }
        let color_eigen = m[color_qingyise];

        for &lianqi_eigen in &[0xe222222200, 0xe022222220, 0xe002222222] {
            if MahjongLogic::is_eigen_include(lianqi_eigen, color_eigen) {
                return true;
            }
        }

        return false;
    }

    #[no_mangle]
    pub extern "C" fn is_jiulianbaodeng(&self, m: Matrix) -> bool {
        let color_qingyise = self.is_qingyise(m);

        if color_qingyise == 99 {
            return false;
        }
        let color_eigen = m[color_qingyise];

        for &jiulian_eigen in &[
            0xe311111114,
            0xe311111123,
            0xe311111213,
            0xe311112113,
            0xe311121113,
            0xe311211113,
            0xe312111113,
            0xe321111113,
            0xe411111113,
        ] {
            if MahjongLogic::is_eigen_include(jiulian_eigen, color_eigen) {
                return true;
            }
        }

        return false;
    }

    fn get_deigens(color: Color, pair_type: PairType) -> Vec<Eigen> {
        match color {
            COLOR_WAN | COLOR_TONG | COLOR_TIAO => match pair_type {
                PairType::MianZi => MIANZI_ARR.to_vec(),
                PairType::DaZi => DAZI_ARR.to_vec(),
                _ => MIANZI_ARR.to_vec(),
            },
            COLOR_FENG => match pair_type {
                PairType::MianZi => FENG_MIANZI_ARR.to_vec(),
                PairType::DaZi => FENG_DAZI_ARR.to_vec(),
                _ => MIANZI_ARR.to_vec(),
            },
            COLOR_JIAN => match pair_type {
                PairType::MianZi => JIAN_MIANZI_ARR.to_vec(),
                PairType::DaZi => JIAN_DAZI_ARR.to_vec(),
                _ => MIANZI_ARR.to_vec(),
            },
            _ => match pair_type {
                PairType::MianZi => MIANZI_ARR.to_vec(),
                PairType::DaZi => DAZI_ARR.to_vec(),
                _ => MIANZI_ARR.to_vec(),
            },
        }
    }

    fn rid_guzhang(color: Color, mut eigen: Eigen, tmp: &mut Vec<Vec<Deigen>>) {
        let path_index = tmp.len() - 1;
        if eigen != 0 {
            for &num in VALID_NUMS[color] {
                let count = MahjongLogic::get_num_cardcount(eigen, num);
                if count > 0 {
                    let deigen = EIGEN_MAP[num];
                    eigen -= deigen;
                    for _ in 0..count {
                        let t = Deigen {
                            deigen: deigen,
                            pair_type: PairType::Guzhang,
                        };
                        tmp[path_index].push(t);
                    }
                }
            }
        }

        tmp.push(Vec::default());
        return;
    }

    fn rid_dazi(color: Color, mut eigen: Eigen, tmp: &mut Vec<Vec<Deigen>>) {
        let mut rid_times = 0;
        let a = MahjongLogic::get_deigens(color, PairType::DaZi);
        for deigen in a {
            if MahjongLogic::is_eigen_include(eigen, deigen) {
                let path_index = tmp.len() - 1;
                let t = Deigen {
                    deigen: deigen,
                    pair_type: PairType::DaZi,
                };

                tmp[path_index].push(t);
                MahjongLogic::rid_dazi(color, eigen, tmp);
                eigen = eigen + deigen;
                rid_times += 1;
            }
        }

        if rid_times == 0 {
            MahjongLogic::rid_guzhang(color, eigen, tmp);
        }
        return;
    }

    fn rid_mianzi(color: Color, mut eigen: Eigen, tmp: &mut Vec<Vec<Deigen>>) {
        let mut rid_times = 0;
        let a = MahjongLogic::get_deigens(color, PairType::MianZi);
        for deigen in a {
            if MahjongLogic::is_eigen_include(eigen, deigen) {
                let path_index = tmp.len() - 1;
                let t = Deigen {
                    deigen: deigen,
                    pair_type: PairType::MianZi,
                };

                tmp[path_index].push(t);
                MahjongLogic::rid_mianzi(color, eigen, tmp);
                eigen = eigen + deigen;
                rid_times += 1;
            }
        }

        if rid_times == 0 {
            MahjongLogic::rid_dazi(color, eigen, tmp);
        }
        return;
    }

    #[no_mangle]
    pub extern "C" fn get_tile_value(&self, m: Matrix, remain_map: HashMap<Card, i32>) -> f32 {
        let mut tile_value = 1.0;

        let mut callback = |_pair_type: &PairType, _cards: Cards, weight_total| {
            tile_value *= weight_total;
        };

        self.split_tile(m, remain_map, &mut callback);
        return tile_value;
    }

    #[no_mangle]
    pub extern "C" fn get_outcard_suggestion(
        &self,
        m: Matrix,
        remain_map: HashMap<Card, i32>,
    ) -> HashMap<Card, f32> {
        let mut ting_cards_info: HashMap<Card, f32> = HashMap::new();

        let mut callback = |pair_type: &PairType, cards: Cards, weight_total| {
            for card in cards {
                let weight = ting_cards_info.entry(card).or_insert(weight_total);
                if let PairType::Guzhang = pair_type {
                } else {
                    *weight *= 1.2;
                }
            }
        };
        self.split_tile(m, remain_map, &mut callback);

        return ting_cards_info;
    }

    fn split_tile(
        &self,
        m: Matrix,
        remain_map: HashMap<Card, i32>,
        callback: &mut dyn FnMut(&PairType, Cards, f32),
    ) {
        let total_cardcount = MahjongLogic::get_total_cardcount(m);

        for &color in &self.valid_colors {
            let color_eigen = m[color];
            if color_eigen == 0 {
                continue;
            }

            let eigen_cardcount = MahjongLogic::get_eigen_cardcount(color_eigen);
            let mut tmp: Vec<Vec<Deigen>> = vec![vec![]];
            MahjongLogic::rid_mianzi(color, color_eigen, &mut tmp);

            for v in &tmp {
                for vv in v {
                    let deigen = vv.deigen;
                    let pair_type = &vv.pair_type;
                    let mut cards = Cards::default();
                    // todo 优化为直接取预存结果
                    for &num in VALID_NUMS[color] {
                        let count = MahjongLogic::get_num_cardcount(deigen, num);
                        if count > 0 {
                            let card = MahjongLogic::make_card(color, num);
                            cards.push(card);
                        }
                    }

                    // 根据花色的数量
                    let mut weight_total = 1.0;
                    let weight_cardcount = eigen_cardcount as f32 / total_cardcount as f32;
                    weight_total *= weight_cardcount;

                    // 根据拆分的类型
                    let weight_pairtype = match pair_type {
                        PairType::MianZi => MIANZI_WEIGHT_MAP[&deigen],
                        PairType::DaZi => DAZI_WEIGHT_MAP[&deigen],
                        PairType::Guzhang => 0.4,
                    };
                    weight_total *= weight_pairtype;

                    // 根据牌的剩余数量
                    let mut lack_cards = Cards::default();
                    match pair_type {
                        PairType::MianZi => {}
                        PairType::DaZi => {
                            lack_cards = DAZI_TING_NUM_MAP[&deigen]
                                .iter()
                                .map(|num| MahjongLogic::make_card(color, *num))
                                .collect()
                        }
                        PairType::Guzhang => {
                            lack_cards = cards.clone();
                        }
                    }
                    if let PairType::MianZi = pair_type {
                    } else {
                        let random_count = 6;
                        let total_count = lack_cards
                            .iter()
                            .map(|card| remain_map.get(card).cloned().unwrap_or(0))
                            .sum::<i32>();
                        let weight_remaincount = total_count as f32 / random_count as f32;
                        if weight_remaincount > 0.0 {
                            weight_total *= weight_remaincount;
                        }
                    }

                    callback(pair_type, cards, weight_total);
                }
            }
        }
    }
}

#[bench]
fn bench_is_hu_speed(b: &mut Bencher) {
    let laizi_cards = vec![0x61];

    let ml = MahjongLogic {
        valid_colors: vec![0, 1, 2],
        laizi_cards_set: laizi_cards.clone().into_iter().collect(),
        contain_qidui: true,
    };

    let cards = vec![
        0x01, 0x01, 0x11, 0x11, 0x11, 0x21, 0x21, 0x21, 0x24, 0x25, 0x26,
    ];

    let m = ml.cards_to_matrix(cards);

    b.iter(|| {
        ml.is_hu(m);
    });

    // let color_eigen = 0x111111;
    // let cardcount = 6;
    // let color = 0;

    // b.iter(|| {
    //     get_eigen_cardcount(color_eigen);
    // });

    // b.iter(||{
    //     is_eigen_match(color_eigen, cardcount, color);
    //     is_eigen_match(color_eigen, cardcount, color);
    //     is_eigen_match(color_eigen, cardcount, color);
    // });

    //    let cards = vec![0x01, 0x21, 0x21, 0x21, 0x01, 0x02, 0x03, 0x11, 0x12, 0x13];
    ////    let cards = vec![0x01, 0x01, 0x01, 0x02];
    //
    //    let m = mahjongLogic::cards_to_matrix(cards);
    //    b.iter(|| {
    //        mahjongLogic::get_ting_cards(m);
    //    });
}

#[test]
fn test_get_ting_cards() {

    let laizi_cards = vec![0x61];
    let ml = MahjongLogic {
        valid_colors: vec![0, 1, 2],
        laizi_cards_set: laizi_cards.clone().into_iter().collect(),
        contain_qidui: true,
    };

    let cards = vec![38,34,34,23,22,21,21,19,19,18,7,5,4,2];
    let m = ml.cards_to_matrix(cards);

    println!("{:?}", ml.get_ting_pairs(m).len());
}

#[test]
fn test_hutable() {
    let laizi_cards = vec![0x61];

    let ml = MahjongLogic {
        valid_colors: vec![0, 1, 2],
        laizi_cards_set: laizi_cards.clone().into_iter().collect(),
        contain_qidui: true,
    };

    let cards = vec![
        0x01, 0x01, 0x11, 0x11, 0x11, 0x21, 0x21, 0x21, 0x24, 0x25, 0x26,
    ];
    let m = ml.cards_to_matrix(cards);
    // assert_eq!(ml.is_hu(m), true);

    let cards = vec![
        0x02, 0x01, 0x11, 0x11, 0x11, 0x21, 0x21, 0x21, 0x24, 0x25, 0x26,
    ];
    let m = ml.cards_to_matrix(cards);
    // assert_eq!(ml.is_hu(m), false);

    let cards = vec![
        0x61, 0x61, 0x02, 0x02, 0x04, 0x05, 0x12, 0x13, 0x24, 0x25, 0x03,
    ];
    let m = ml.cards_to_matrix(cards);
    println!("{:?}", m);
    assert_eq!(ml.is_hu(m), true);

    // print_hutable(&BASE_HUMAP, &Path::new("BaseHuTable.lua"));
    // print_hutable(&FENG_HUMAP, &Path::new("FengHuTable.lua"));
    // print_hutable(&LAIZI_HUMAP, &Path::new("LaiziHuTable.lua"));
}
