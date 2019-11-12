# Unity_Train_RainbowTower

## Unity练手项目 Android 仿反应堆。加入简易的网络联机功能，采用的是Photon PUN2  

### Photon 网址：<https://dashboard.photonengine.com/>

#### Unity版本号：2019.2.8f1

连接Photon服务器，需要在 Others -> Photon --> PhotonUnityNetworking --> Resources --> PhotonServerSettings.asset 中的Settings下APP Id Realtime 填写申请的id号

中国区id处理：<https://vibrantlink.com/chinacloudpun/>

反应堆参考文献：<https://zhuanlan.zhihu.com/p/38359628>

主要文件目录:

- apks:生成的apk
- RainbowTower：&ensp;&ensp;Unity项目文件夹
  - Assets:&ensp;&ensp;资源
    - Materials：&ensp;&ensp;材质球
    - Others:&ensp;&ensp;Photon插件
    - Prefabs:&ensp;&ensp;预设体
    - Resources:&ensp;&ensp;动态资源加载（实际上仅用到BasePlate.prefab和NetTransform.prefab）
    - Scenes:&ensp;&ensp;场景
    - Scripts：&ensp;&ensp;脚本
      - LS:&ensp;&ensp;自己以往的代码
      - Network:&ensp;&ensp;网络脚本
    - Settings：&ensp;&ensp;继承自ScriptsObject的配置文件
    - Textures：&ensp;&ensp;贴图（仅有一张遮罩贴图）
