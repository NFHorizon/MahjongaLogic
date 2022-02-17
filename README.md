# MahjongaLogic（mahjong algorithm lib）
基于查表法的麻将算法库，支持带癞子听牌算法，且能计算出听的牌的基础番型

c#（cs）版本用法
1. 自建c#工程运行或直接在已有C#项目中包含，unity或.NET都支持
2. 调用库方法之前，需要初始化查表法所用到的表，为什么库不直接做这个工作呢，是因为生成所需的表较为耗时，如果使用已缓存的表，则可以大大优化初始化速度，因此留给用户来选择
  - 你可以调用`MahjongaLogic.Init(MahjongLogic.gen_laizimap(0, 7))` 来进行初始化
  - 然后构造一个MahjongaLogic的实例来调用方法（这是为了适配不同的游戏中麻将算法的配置不同的情况）
3. 现在你就可以自由调用库中的函数了，如下是一段用于测试一副手牌是否是九莲宝灯的测试代码
  ```csharp
//测试九莲宝灯
List<int> cards = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 1, 1, 9, 9, 2 };
long[] m = mahjongLogic.cards_to_matrix(cards);

var is_jiulianbaodeng = mahjongLogic.is_jiulianbaodeng(m);
Assert.True(is_jiulianbaodeng);

cards = new() { 0x61, 0x61, 1, 4, 4, 0x15, 0x15, 0x19, 0x19, 0x21, 0x21, 0x27, 0x28, 6 };
m = mahjongLogic.cards_to_matrix(cards);

Assert.False(mahjongLogic.is_qidui(m));

cards = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 1, 1, 1, 9, 9 };
m = mahjongLogic.cards_to_matrix(cards);
is_jiulianbaodeng = mahjongLogic.is_jiulianbaodeng(m);
Assert.True(is_jiulianbaodeng);
```
