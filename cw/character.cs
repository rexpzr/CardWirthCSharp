//import os
//import copy
//import math
//import shutil
//import itertools
//import threading
//
//import cw
//from cw.util import synclock
//
//
//_couponlock = threading.Lock()

class Character
{          
    private UNK data;
    private bool reversed;
    private string name;
    private int level;
    private List<UNK> cardpocket;
    private List<UNK> hold_all;
    private UNK life;
    private UNK maxlife;
    private string mentality;
    private UNK mentality_dur;
    private UNK paralyze;
    private UNK poison;
    private UNK bind;
    private UNK silence;
    private UNK faceup;
    private UNK antimagic;
    private UNK enhance_act;
    private UNK enhance_act_dur;
    private UNK enhance_avo;
    private UNK enhance_avo_dur;
    private UNK enhance_res;
    private UNK enhance_res_dur;
    private UNK enhance_def;
    private UNK enhance_def_dur;
    private UNK physical;
    private UNK mental;
    private UNK enhance;
    private UNK feature;
    private UNK noeffect;
    private UNK resist;
    private UNK weakness;
    private Deck deck;
    private UNK actiondata;
    private bool actionautoselected;
    private UNK actionorder;
    private bool actionend;
    private bool reversed;
    private Dictionary<UNK, UNK> coupons;
    private UNK timedcoupons;
    private bool _vanished;
    private UNK versionhint;
    private UNK cardimg;
    private UNK test_aptitude;
    private Dictionary<UNK, UNK> _voc_tbl;


    public Character(UNK? data=null)
    {
        if (data != null) {
            this.data = data;
            this.reversed = false;
        }

        // 名前
        this.name = this.data.gettext("Property/Name", "");
        // レベル
        this.level = cw.util.numwrap(this.data.getint("Property/Level", 1), 1, 65536);
        // 各種所持カードのリスト
        this.cardpocket = this.get_cardpocket();
        // 全てホールド
        this.hold_all = [
           this.data.getbool("SkillCards", "hold_all", false),
           this.data.getbool("ItemCards", "hold_all", false),
           this.data.getbool("BeastCards", "hold_all", false),
        ];
        // 現在ライフ・最大ライフ
        this.life = max(0, this.data.getint("Property/Life", 0));
        this.maxlife = max(1, this.data.getint("Property/Life", "max", 1));
        this.life = min(this.maxlife, this.life);
        // 精神状態
        this.mentality = this.data.gettext("Property/Status/Mentality", "Normal");
        this.mentality_dur = cw.util.numwrap(this.data.getint("Property/Status/Mentality",
                                                               "duration", 0), 0, 999);

        if (this.mentality_dur == 0 || this.mentality == "Normal")
        {
             this.mentality = "Normal";
             this.mentality_dur = 0; 
        }

        // 麻痺値
        this.paralyze = cw.util.numwrap(this.data.getint("Property/Status/Paralyze", 0), 0, 40);
        // 中毒値
        this.poison = cw.util.numwrap(this.data.getint("Property/Status/Poison", 0), 0, 40);
        // 束縛時間値
        this.bind = cw.util.numwrap(this.data.getint("Property/Status/Bind", "duration", 0), 0, 999);
        // 沈黙時間値
        this.silence = cw.util.numwrap(this.data.getint("Property/Status/Silence", "duration", 0), 0, 999);
        // 暴露時間値
        this.faceup = cw.util.numwrap(this.data.getint("Property/Status/FaceUp", "duration", 0), 0, 999);
        // 魔法無効時間値
        this.antimagic = cw.util.numwrap(this.data.getint("Property/Status/AntiMagic",
                                                                   "duration", 0), 0, 999);
        // 行動力強化値
        this.enhance_act = cw.util.numwrap(this.data.getint("Property/Enhance/Action", 0), -10, 10);
        this.enhance_act_dur = cw.util.numwrap(this.data.getint("Property/Enhance/Action",
                                                                   "duration", 0), 0, 999);
        if (this.enhance_act == 0 || this.enhance_act_dur == 0)
        {
           this.enhance_act = 0;
           this.enhance_act_dur = 0;
        }

        // 回避力強化値
        this.enhance_avo = cw.util.numwrap(this.data.getint("Property/Enhance/Avoid", 0), -10, 10);
        this.enhance_avo_dur = cw.util.numwrap(this.data.getint("Property/Enhance/Avoid",
                                                                   "duration", 0), 0, 999);
        if (this.enhance_avo == 0 || this.enhance_avo_dur == 0)
        {
           this.enhance_avo = 0;
           this.enhance_avo_dur = 0;
        }
        // 抵抗力強化値
        this.enhance_res = cw.util.numwrap(this.data.getint("Property/Enhance/Resist", 0), -10, 10);
        this.enhance_res_dur = cw.util.numwrap(this.data.getint("Property/Enhance/Resist",
                                                                   "duration", 0), 0, 999);
        if (this.enhance_res == 0 || this.enhance_res_dur == 0)
        {
           this.enhance_res = 0;
           this.enhance_res_dur = 0;
        }
        // 防御力強化値
        this.enhance_def = cw.util.numwrap(this.data.getint("Property/Enhance/Defense", 0), -10, 10);
        this.enhance_def_dur = cw.util.numwrap(this.data.getint("Property/Enhance/Defense",
                                                                   "duration", 0), 0, 999);
        if (this.enhance_def == 0 || this.enhance_def_dur == 0)
        {
            this.enhance_def = 0;
            this.enhance_def_dur = 0;

        }
        // 各種能力値
        e = this.data.getfind("Property/Ability/Physical");
        this.physical = copy.copy(e.attrib);
        e = this.data.getfind("Property/Ability/Mental");
        this.mental = copy.copy(e.attrib);
        e = this.data.getfind("Property/Ability/Enhance");
        this.enhance = copy.copy(e.attrib);

        foreach(var item in this.physical) {
            try {
                this.physical[item.Key] = cw.util.numwrap((float)item.Value, 0, 65536);
            } catch(Exception e) {
                this.physical[item.Key] = 0;
            }
        }

        foreach(var item in this.mental) {
            try {
                this.mental[item.Key] = cw.util.numwrap((float)item.Value, -65536, 65536);
            } catch(Exception e) {
                this.mental[item.Key] = 0;
            }
        }

        foreach(var item in this.enhance) {
            try {
                this.enhance[item.Key] = cw.util.numwrap((float)item.Value, -10, 10);
            } catch(Exception e) {
                this.enhance[item.Key] = 0;
            }
        }

        // 特性
        e = this.data.getfind("Property/Feature/Type");
        this.feature = copy.copy(e.attrib);
        e = this.data.getfind("Property/Feature/NoEffect");
        this.noeffect = copy.copy(e.attrib);
        e = this.data.getfind("Property/Feature/Resist");
        this.resist = copy.copy(e.attrib);
        e = this.data.getfind("Property/Feature/Weakness");
        this.weakness = copy.copy(e.attrib);

        // for d in (this.feature, this.noeffect, this.resist, this.weakness):
        //     for key, value in d.iteritems():
        //         try:
        //             d[key] = cw.util.str2bool(value)
        //         except:
        //             d[key] = false

        // デッキ
        this.deck = cw.deck.Deck(self);
        // 戦闘行動(Target, CardHeader)
        this.actiondata = None;
        this.actionautoselected = false;
        // 行動順位を決定する数値
        this.actionorder = 0;
        // ラウンド処理中で行動開始前ならTrue
        this.actionend = true;

        this.reversed = false;

        // クーポン一覧
        this.coupons = {};

        // for e in this.data.getfind("Property/Coupons"):
        //     if not e.text:
        //         continue

        //     if e.text in (u"＠効果対象", u"イベント対象", u"使用者"):
        //         // 効果・イベント対象に付与されるシステムクーポン(Wsn.2)
        //         continue

        //     try:
        //         this.coupons[e.text] = int(e.get("value")), e
        //     except:
        //         this.coupons[e.text] = 0, e
        //     if e.text == u"：Ｒ":
        //         this.reversed = True

        // 時限クーポンのデータのリスト(name, flag_countable)
        this.timedcoupons = this.get_timedcoupons();

        // 対象消去されたか否か
        this._vanished = false;
        // 互換性マーク
        this.versionhint = cw.cwpy.sct.from_basehint(this.data.getattr("Property", "versionHint", ""));

        // 状態の正規化
        this.cardimg = null;

        if (this.is_unconscious()) {
            // 最初から意識不明の場合、基本的に全てのステータスが
            // クリアされるが、唯一、回数制限つきの付帯能力だけは、
            // 後から意識不明になった時と違ってクリアされない(CardWirth 1.50)
            this.set_unconsciousstatus(clearbeast=false);
        }

        // 適性検査用のCardHeader。
        this.test_aptitude = None;

        // キャッシュ
        this._voc_tbl = {};
    }

    public UNK get_imagepaths()
    {
        // 現在表示中のカード画像の情報を
        // cw.image.ImageInfoのlistで返す。
        UNK data = this.data.find("Property");
        return cw.image.get_imageinfos(data);
    }

    public UNK set_images(UNK paths)
    {
        // """このキャラクターのカード画像を
        // cw.image.ImageInfoのlistで指定した内容に差し替える。
        // """
        if (cw.cwpy.ydata)
        {
            cw.cwpy.ydata.changed();
        }
        etree = null;
        eimg = null;
        infos = this.get_imagepaths();
        can_loaded_scaledimage = this.data.getattr(".", "scaledimage", false);
        if (infos)
        {
            if (cw.cwpy.is_playingscenario())
            {
                // メッセージログのイメージが変化しないように
                // ファイル上書き前に読み込んでおく
                foreach (var info in infos)
                {
                    if (!info.path)
                    {
                        continue;
                    }
                    fpath = info.path;
                    fname = os.path.basename(fpath);
                    fpath2 = cw.util.join_yadodir(fpath);
                    if (os.path.isfile(fpath2));
                    {
                        cw.sprite.message.store_messagelogimage(fpath2, can_loaded_scaledimage);
                    }
                }
                // F9のためにシナリオ突入時の画像の記録を取る
                name = os.path.splitext(os.path.basename(this.data.fpath))[0];
                log = cw.util.join_paths(cw.tempdir, u"ScenarioLog/Face/Log.xml");
                if (os.path.isfile(log));
                {
                    etree = cw.data.xml2etree(log);
                }else{
                    e = cw.data.make_element("FaceLog", "");
                    etree = cw.data.xml2etree(element=e);
                    etree.fpath = log;
                }
                bool _for_else = true;
                foreach (var e in etree.getfind(".", raiseerror=false))
                {
                    member = e.getattr(".", "member");
                    if (member == name)
                    {
                        // すでに記録済み
                        eimg = e;
                        _for_else = false;
                        break;
                    }
                }
                if (_for_else) {
                    e = cw.data.make_element("ImagePaths", u"", {"member":name});
                    eimg = e;
                    dpath = cw.util.join_paths(cw.tempdir, u"ScenarioLog/Face");
                    foreach (var info in infos)
                    {
                        if (!info.path)
                        {
                            continue;
                        }
                        fpath = info.path;
                        fname = os.path.basename(fpath);
                        fpath2 = cw.util.join_yadodir(fpath);
                        if (os.path.isfile(fpath2))
                        {
                            fpath = cw.util.join_paths(dpath, fname);
                            fpath = cw.util.dupcheck_plus(fpath, yado=false);
                            if (!os.path.isdir(dpath))
                            {
                                os.makedirs(dpath);
                            }
                            cw.util.copy_scaledimagepaths(fpath2, fpath, can_loaded_scaledimage);
                            e2 = cw.data.make_element("ImagePath", os.path.basename(fpath));
                            info.set_attr(e2);
                            e.append(e2);
                        }
                    }

                    etree.getroot().append(e);
                }
            }

            foreach (var info in infos)
            {
                if (!info.path)
                {
                    continue;
                }
                fpath = cw.util.join_yadodir(info.path);
                foreach (var fpath, _scale in cw.util.get_scaledimagepaths(fpath, can_loaded_scaledimage))
                {
                    cw.cwpy.ydata.deletedpaths.add(fpath, forceyado=true);
                }
            }
        }
        if (!eimg == null)
        {
            // 複数回変更された時は変更後ファイル情報を
            // 都度最新に更新しておく
            foreach (var e in list(eimg))
            {
                if (e.tag == "NewImagePath")
                {
                    eimg.remove(e);
                }
            }
        }

        // 新しいファイル群をコピー
        newpaths = cw.xmlcreater.write_castimagepath(this.get_name(), paths, true);
        prop = this.data.find("Property");
        foreach (var ename in ("ImagePath", "ImagePaths"))
        {
            e = prop.find(ename);
            if (!e == null)
            {
                prop.remove(e);
            }
        }
        // コピー後のファイルパスを設定
        e = cw.data.make_element("ImagePaths", "");
        prop.append(e);
        foreach (var info in newpaths)
        {
            if (info.path)
            {
                e2 = cw.data.make_element("ImagePath", info.path);
                info.set_attr(e2);
                e.append(e2);

                if (!eimg == null)
                {
                    // F9時に変更後のイメージを削除するため、記録しておく
                    eimg.append(cw.data.make_element("NewImagePath", info.path));
                }
            }
        }

        // 外部から設定したイメージは常にスケーリング可能とする
        this.data.edit(".", (string)(True), "scaledimage"); // TODO

        this.data.is_edited = true;

        if (!etree == null)
        {
            etree.write();
        }

        return newpaths;
    }
    
    public string get_name()
    {       
        return this.data.gettext("Property/Name", "");
    }

    public void set_name(string name){
        if (cw.cwpy.ydata){
            cw.cwpy.ydata.changed();
        }
        this.data.edit("Property/Name", name);
        this.name = name;
    }

    public string get_description()
    {
        return cw.util.decodewrap(this.data.gettext("Property/Description", ""));
    }

    public void set_description(UNK desc)
    {
        if (cw.cwpy.ydata)
        {
            cw.cwpy.ydata.changed();
        }
        this.data.edit("Property/Description", cw.util.encodewrap(desc));
    }

    public void set_maxlife(UNK value)
    {
        if (cw.cwpy.ydata)
        {
            cw.cwpy.ydata.changed();
        }
        v = float(this.life) / this.maxlife;
        this.maxlife = value;
        this.data.edit("Property/Life", str(int(value)), "max");
        if (this.life != 0)
        {
            this.life = max(1, int(this.maxlife * v));
            this.data.edit("Property/Life", str(int(this.maxlife)));
        }
        if (this.data.getattr("Property/Life", "coefficient", 0))
        {
            this.data.remove("Property/Life", attrname="coefficient");
        }
    }

    public void set_physical(UNK name, UNK value)
    {
        if (cw.cwpy.ydata)
        {
            cw.cwpy.ydata.changed();
        }
        this.data.edit("Property/Ability/Physical", str(int(value)), name);
        this.physical[name] = float(value);
        this._clear_vocationcache();
    }

    public void set_mental(UNK name, UNK value)
    {
        if (cw.cwpy.ydata)
        {
            cw.cwpy.ydata.changed();
        }
        this.data.edit("Property/Ability/Mental", str(value), name);
        this.mental[name] = float(value);
        this._clear_vocationcache();
    }

  public void set_feature(UNK name, UNK value)
  {
      if (cw.cwpy.ydata)
      {
          cw.cwpy.ydata.changed();
      }
      this.data.edit("Property/Feature/Type", str(value), name);
      this.feature[name] = value;
  }
  public void set_noeffect(UNK name, UNK value)
  {
      if (cw.cwpy.ydata)
      {
          cw.cwpy.ydata.changed();
      }
      this.data.edit("Property/Feature/NoEffect", str(value), name);
      this.noeffect[name] = value;
  }
  public void set_resist(UNK name, UNK value)
  {
      if (cw.cwpy.ydata)
      {
          cw.cwpy.ydata.changed();
      }
      this.data.edit("Property/Feature/Resist", str(value), name);
      this.resist[name] = value;
  }
  public void set_weakness(UNK name, UNK value)
  {
      if (cw.cwpy.ydata)
      {
          cw.cwpy.ydata.changed();
      }
      this.data.edit("Property/Feature/Weakness", str(value), name);
      this.weakness[name] = value;
  }
  public void set_enhance(UNK name, UNK value)
  {
      if (cw.cwpy.ydata)
      {
          cw.cwpy.ydata.changed();
      }
      this.data.edit("Property/Ability/Enhance", str(value), name);
      this.enhance[name] = value;
  }

    public UNK get_cardpocket() {
        bool flag = (bool)(this.data.getroot().tag == "CastCard");
        UNK maxnums = this.get_cardpocketspace();
        UNK paths = ("SkillCards", "ItemCards", "BeastCards"); //TODO
        var cardpocket = new List<UNK>();

        foreach(var maxn, path in zip(maxnums, paths)) { //TODO
            var headers = new List<UNK>();

            pe = this.data.find(path);
            if (pe != null) {
                foreach(var e in pe) {
                    if (maxn <= len(headers)) {
                        // 最大所持数を越えたカードは消去
                        break;
                    }
                    e = cw.cwpy.sdata.get_carddata(e, inusecard=false); //TODO
                    if (e == null) {
                        continue;
                    }
                    header = cw.header.CardHeader(owner=self, carddata=e, from_scenario=flag); //TODO
                    headers.Add(header);
                }

                // 参照先に差し替えられている可能性があるので
                // ここでpeの子要素を入れ替える
                foreach(var e in list(pe)) { //TODO
                    pe.Remove(e); //TODO
                }
                foreach(var header in headers) {
                    pe.Add(header.carddata);
                }
            }

            cardpocket.append(headers[:maxn]);
        }

        return tuple(cardpocket);
    }

    public UNK get_keycodes(bool skill=true, bool item=true, bool beast=true)) {
        """所持カードのキーコード一覧を返す。"""
        s = set(); //TODO
        List<UNK> seq = new List<UNK>(); //TODO
        if (skill) {
            seq.Add(this.get_pocketcards(cw.POCKET_SKILL));
        }
        if (item) {
            seq.Add(this.get_pocketcards(cw.POCKET_ITEM));
        }
        if (beast) {
            seq.Add(this.get_pocketcards(cw.POCKET_BEAST));
        }

        foreach(var header in seq) {
            s.update(header.get_keycodes());
        }

        s.discard("");
        return s;
    }

    public bool has_keycode(UNK keycode, bool skill=true, bool item=true, bool beast=true, bool hand=true)) {
        """指定されたキーコードを所持しているか。"""
        if (hand && this.deck) {
            // 戦闘時の手札(Wsn.2);
            foreach(var header in this.deck.get_hand(self)) {
                if (keycode in header.get_keycodes()) { //TODO
                    return true;
                }
            }
        }
        if (skill) {
            foreach(var header in this.get_pocketcards(cw.POCKET_SKILL)) {
                if (keycode in header.get_keycodes()) { //TODO
                    return true;
                }
            }
        }
        if (item) {
            foreach(var header in this.get_pocketcards(cw.POCKET_ITEM)) {
                if (keycode in header.get_keycodes()) { //TODO
                    return true;
                }
            }
        }
        if (beast) {
            foreach(var header in this.get_pocketcards(cw.POCKET_BEAST)) {
                if (keycode in header.get_keycodes()) { //TODO
                    return true;
                }
            }
        }

        return false;
    }

   public virtual UNK lost(){
       // """
       // 対象消去やゲームオーバー時に呼ばれる。
       // Playerクラスでオーバーライト。
       // """
   }
    // ---------------------------------------------------------------------------
    // 　状態チェック用
    // ---------------------------------------------------------------------------
    public bool is_normal()
    {
        // """
        // 通常の精神状態かどうかをbool値で返す。
        // """
        return bool(this.mentality == "Normal");
    }

    public bool is_panic()
    {
        // """
        // 恐慌状態かどうかをbool値で返す
        // """
        return bool(this.mentality == "Panic");
    }

    public bool is_brave()
    {
        // """
        // 勇敢状態かどうかをbool値で返す
        // """
        return bool(this.mentality == "Brave");
    }

    public bool is_overheat()
    {
        // """
        // 激昂状態かどうかをbool値で返す
        // """
        return bool(this.mentality == "Overheat");
    }

    public bool is_confuse()
    {
        // """
        // 混乱状態かどうかをbool値で返す
        // """
        return bool(this.mentality == "Confuse");
    }

    public bool is_sleep()
    {
        // """
        // 睡眠状態かどうかをbool値で返す
        // """
        return bool(this.mentality == "Sleep");
    }

    public bool is_paralyze()
    {
        // """
        // 麻痺または石化状態かどうかをbool値で返す
        // """
        return bool(this.paralyze > 0);
    }

    public bool is_poison()
    {
        // """
        // 中毒状態かどうかをbool値で返す
        // """
        return bool(this.poison > 0);
    }

    public bool is_bind()
    {
        // """
        // 呪縛状態かどうかをbool値で返す
        // """
        return bool(this.bind > 0);
    }

    public bool is_silence()
    {
        // """
        // 沈黙状態かどうかをbool値で返す。
        // """
        return bool(this.silence > 0);
    }

    public bool is_faceup()
    {
        // """
        // 暴露状態かどうかをbool値で返す。
        // """
        return bool(this.faceup > 0);
    }

    public bool is_antimagic()
    {
        // """
        // 魔法無効状態かどうかをbool値で返す。
        // """
        return bool(this.antimagic > 0);;
    }

    public static bool calc_petrified(UNK paralyze)
    {
        return paralyze > 20;
    }

    public bool is_petrified()
    {
        // """
        // 石化状態かどうかをbool値で返す
        // """
        return Character.F.calc_petrified(this.paralyze);
    }

    public bool is_unconscious()
    {
        // """
        // 意識不明状態かどうかをbool値で返す
        // """
        return this.life <= 0;
    }

    public bool calc_heavyinjured(UNK life, UNK maxlife)
    {
        return Character.F.calc_lifeper(life, maxlife) <= 20 && 0 < life;
    }

    public bool is_heavyinjured()
    {
        // """
        // 重傷状態かどうかをbool値で返す
        // """
        return Character.calc_heavyinjured(this.life, this.maxlife);
    }

    public static bool calc_injured(UNK life,UNK maxlife)   {
        return (bool)life < maxlife && !Character.calc_heavyinjured(life, maxlife) && 0 < life;
    }

    public bool is_injured()
    {
        // """
        // 軽傷状態かどうかをbool値で返す
        // """
        return Character.calc_injured(this.life, this.maxlife);
    }

    public bool is_injuredall()
    {
        // """
        // 負傷状態かどうかをbool値で返す
        // """
        return (bool)this.life < this.maxlife;
    }
//
//    def is_inactive(self, check_reversed=True):
//        """
//        行動不可状態かどうかをbool値で返す
//        """
//        b = self.is_sleep()
//        b |= self.is_paralyze()
//        b |= self.is_bind()
//        b |= self.is_unconscious()
//        if check_reversed:
//            b |= self.is_reversed()
//        return b
//
//    def is_active(self):
//        """
//        行動可能状態かどうかをbool値で返す
//        """
//        return not self.is_inactive()
//
//    def is_dead(self):
//        """
//        非生存状態かどうかをbool値で返す
//        """
//        b = self.is_paralyze()
//        b |= self.is_unconscious()
//        b |= self.is_reversed()
//        return b
//
//    def is_alive(self):
//        """
//        生存状態かどうかをbool値で返す
//        """
//        return not self.is_dead()

    public UNK is_fine()
    {
        // """
        // 健康状態かどうかをbool値で返す
        // """
        return !this.is_injuredall() && !this.is_unconscious();
    }

    public UNK is_analyzable()
    {
        // """
        // 各種データが暴露可能かどうかbool値で返す。
        // EnemyCardのための処理。
        // デバッグフラグがTrueだったら問答無用で暴露する。
        // """
        if (isinstance(Enemy))
        {
            return cw.cwpy.debug || this.is_faceup();
        }else{
            return true;
        }
    }

    public UNK is_avoidable(bool use_enhance=true)
    {
        // """
        // 回避判定可能かどうかbool値で返す。
        // """
        if (use_enhance)
        {
            return this.is_active() && this.get_enhance_avo() > -10;
        }else{
            return this.is_active();
        }
    }
//    def is_resistable(self, use_enhance=True):
//        """
//        抵抗判定可能かどうかbool値で返す。
//        呪縛状態でも抵抗できる。
//        """
//        b = self.is_sleep()
//        b |= self.is_paralyze()
//        b |= self.is_unconscious()
//        if use_enhance:
//            return not b and self.get_enhance_res() > -10
//        else:
//            return not b
//
    public bool is_reversed()
    {
        // """
        // 隠蔽状態かどうかbool値で返す。
        // """
        return this.reversed;
    }

    public bool is_vanished()
    {
        return this._vanished;
    }

    public int has_beast()
    {
        // """
        // 付帯召喚じゃない召喚獣カードの所持数を返す。
        // """
        int counter = 0;
        foreach(var h in this.get_pocketcards(cw.POCKET_BEAST)) {
            if (!h.attachment) {
                counter++;
            }
        }
        return coutner;
    }
    public bool is_enhanced_act()
    {
        return this.enhance_act != 0 && 0 < this.enhance_act_dur;
    }
    public bool is_enhanced_res()
    {
        return this.enhance_res != 0 && 0 < this.enhance_res_dur;
    }
    public bool is_enhanced_avo()
    {
        return this.enhance_avo != 0 && 0 < this.enhance_avo_dur;
    }
    public bool is_enhanced_def()
    {
        return this.enhance_def != 0 && 0 < this.enhance_def_dur;
    }
    public bool is_upaction()
    {
        return this.enhance_act > 0 && 0 < this.enhance_act_dur;
    }
    public bool is_upresist()
    { 
        return this.enhance_res > 0 && 0 < this.enhance_res_dur;
    }
    public bool is_upavoid()
    {
        return this.enhance_avo > 0 && 0 < this.enhance_avo_dur;
    }
    public bool is_updefense()
    {
        return this.enhance_def > 0 && 0 < this.enhance_def_dur;
    }
    public bool is_downaction()
    {
        return this.enhance_act < 0 && 0 < this.enhance_act_dur;
    }
    public bool is_downresist()
    {
        return this.enhance_res < 0 && 0 < this.enhance_res_dur;
    }
    public bool is_downavoid()
    {
        return this.enhance_avo < 0 && 0 < this.enhance_avo_dur;
    }
    public bool is_downdefense()
    {
        return this.enhance_def < 0 && 0 < this.enhance_def_dur;
    }
    

    public bool is_effective(UNK motion)
    {
        // """motionが現在のselfに対して有効な効果か。
        // ターゲットの選択に使用される判定であるため、
        // 実際には有効であっても必ずしもtrueを返さない。
        // """
        int value;
        string mtype;

        if (this.is_reversed() || this.is_vanished() || (this.status == "hidden" && !isinstance((Friend, Player))))
        {
            return false;
        }
        if (cw.effectmotion.is_noeffect(motion.get("element", "")))
        {
            return false;
        }
        mtype = motion.get("type", "");
        if (mtype == "Heal")
        {
            return this.is_injuredall();
        }else if (mtype == "Damage"){
            return !this.is_unconscious();
        }else if (mtype == "Absorb"){
            return !this.is_unconscious();
        }else if (mtype == "Paralyze"){
            return !this.is_unconscious();
        }else if (mtype == "DisParalyze"){
            return this.is_paralyze();
        }else if (mtype == "Poison"){
            return !this.is_unconscious();
        }else if (mtype == "DisPoison"){
            return this.is_poison();
        }else if (mtype == "GetSkillPower"){
            return !this.is_unconscious();
        }else if (mtype == "LoseSkillPower"){
            return !this.is_unconscious();
        }else if (mtype in "Sleep"){
            // CardWirthでは、すでに睡眠状態なら
            // さらに大きな時間で上書き可能な状態でも
            // ターゲットにしない。
            // 混乱・激昂・勇敢・恐慌・呪縛・沈黙
            // ・暴露・魔法無効も同様
            return !this.is_unconscious() && !this.is_sleep();
        }else if (mtype == "Confuse"){
            return !this.is_unconscious() && !this.is_confuse();
        }else if (mtype == "Overheat"){
            return !this.is_unconscious() && !this.is_overheat();
        }else if (mtype == "Brave"){
            return !this.is_unconscious() && !this.is_brave();
        }else if (mtype == "Panic"){
            return !this.is_unconscious() && !this.is_panic();
        }else if (mtype == "Normal"){
            return !this.is_unconscious() && !this.is_normal();
        }else if (mtype == "Bind"){
            return !this.is_unconscious() && !this.is_bind();
        }else if (mtype == "DisBind"){
            return this.is_bind();
        }else if (mtype == "Silence"){
            return !this.is_unconscious() && !this.is_silence();
        }else if (mtype == "DisSilence"){
            return this.is_silence();
        }else if (mtype == "FaceUp"){
            return !this.is_unconscious() && !this.is_faceup();
        }else if (mtype == "FaceDown"){
            return this.is_faceup();
        }else if (mtype == "AntiMagic"){
            return !this.is_unconscious() && !this.is_antimagic();
        }else if (mtype == "DisAntiMagic"){
            return this.is_antimagic();
        }else if (mtype == "EnhanceAction"){
            // 能力ボーナスは時間を見ず、値のみを見て判定する
            if (this.is_unconscious())
            {
                return false;
            }
            value = motion.getint(".", "value", 0);
            if (value == 0)
            {
                return this.is_enhanced_act();
            }else if (value < 0){
                return value < this.get_enhance_act();
            }else if (0 < value){
                return this.get_enhance_act() < value;
            }
        }else if (mtype == "EnhanceAvoid"){
            if (this.is_unconscious())
            {
                return false;
            }
            value = motion.getint(".", "value", 0);
            if (value == 0)
            {
                return this.is_enhanced_avo();
            }else if (value < 0){
                return value < this.get_enhance_avo();
            }else if (0 < value){
                return this.get_enhance_avo() < value;
            }
        }else if (mtype == "EnhanceResist"){
            if (this.is_unconscious())
            {
                return false;
            }
            value = motion.getint(".", "value", 0);
            if (value == 0)
            {
                return this.is_enhanced_res();
            }else if (value < 0){
                return value < this.get_enhance_res();
            }else if (0 < value){
                return this.get_enhance_res() < value;
            }
        }else if (mtype == "EnhanceDefense"){
            if (this.is_unconscious())
            {
                return false;
            }
            value = motion.getint(".", "value", 0);
            if (value == 0)
            {
                return this.is_enhanced_def();
            }else if (value < 0){
                return value < this.get_enhance_def();
            }else if (0 < value){
                return this.get_enhance_def() < value;
            }
        }else if (mtype == "VanishCard"){
            return this.is_active();
        }else if (mtype == "VanishBeast"){
            return this.has_beast();
        }else if (mtype == "DealAttackCard"){
            return this.is_active();
        }else if (mtype == "DealPowerfulAttackCard"){
            return this.is_active();
        }else if (mtype == "DealCriticalAttackCard"){
            return this.is_active();
        }else if (mtype == "DealFeintCard"){
            return this.is_active();
        }else if (mtype == "DealDefenseCard"){
            return this.is_active();
        }else if (mtype == "DealDistanceCard"){
            return this.is_active();
        }else if (mtype == "DealConfuseCard"){
            return this.is_active();
        }else if (mtype == "DealSkillCard"){
            return this.is_active();
        }else if (mtype == "CancelAction"){ // 1.50
            return this.is_active();
        }else if (mtype == "SummonBeast"){
            return this.can_addbeast();
        }else if (mtype == "NoEffect"){ // Wsn.2
            return !this.is_unconscious();
        }else{
            // VanishTarget: 常に有効
            return true;
        }
     }

    //---------------------------------------------------------------------------
    //　カード操作
    //---------------------------------------------------------------------------

    public UNK use_card(UNK targets, UNK header)
    {
        // """targetsにカードを使用する。"""
        cw.cwpy.advlog.use_card(this, header, targets);

        if (cw.cwpy.ydata)
        {
            cw.cwpy.ydata.changed();
        }
        if (!isinstance(targets, list))
        {
            targets = [targets];
        }

        data = header.carddata;
        // 他の使用中カード削除
        cw.cwpy.clear_inusecardimg();
        // TargetArrow削除
        cw.cwpy.clear_targetarrow();
        // 効果音ファイルのパスを取得
        soundpath = data.gettext("Property/SoundPath", "");
        volume = data.getint("Property/SoundPath", "volume", 100);
        loopcount = data.getint("Property/SoundPath", "loopcount", 1);
        channel = data.getint("Property/SoundPath", "channel", 0);
        fade = data.getint("Property/SoundPath", "fadein", 0);

        // 使用アニメーション
        cw.cwpy.event.in_inusecardevent = true;
        removeafter = false;
        battlespeed = cw.cwpy.is_battlestatus();
        if (header.type == "BeastCard")
        {
            cw.cwpy.set_inusecardimg(this, header, "hidden", center=true);
            inusecardimg = cw.cwpy.get_inusecardimg();
            cw.animation.animate_sprite(inusecardimg, "deal", battlespeed=battlespeed);
            // 効果音を鳴らす
            cw.cwpy.play_sound_with(soundpath, header, subvolume=volume, loopcount=loopcount, channel=channel, fade=fade);

            if (cw.cwpy.setting.enlarge_beastcardzoomingratio)
            {
                cw.animation.animate_sprite(inusecardimg, "zoomin_slow", battlespeed=battlespeed);
            }else{
                inusecardimg.zoomsize_noscale = (16, 22);
                cw.animation.animate_sprite(inusecardimg, "zoomin", battlespeed=battlespeed);
            }

            if (cw.cwpy.setting.wait_usecard)
            {
                waitrate = cw.cwpy.setting.get_dealspeed(cw.cwpy.is_battlestatus())*2+1;
                cw.cwpy.wait_frame(waitrate, cw.cwpy.setting.can_skipanimation);
            }else{
                waitrate = cw.cwpy.setting.get_dealspeed(cw.cwpy.is_battlestatus())+1;
                cw.cwpy.wait_frame(waitrate, cw.cwpy.setting.can_skipanimation);
            }

            if (cw.cwpy.setting.enlarge_beastcardzoomingratio)
            {
                cw.animation.animate_sprite(inusecardimg, "zoomout_slow", battlespeed=battlespeed);
            }else{
                cw.animation.animate_sprite(inusecardimg, "zoomout", battlespeed=battlespeed);
            }

            cw.animation.animate_sprite(inusecardimg, "hide", battlespeed=battlespeed);
        }else if (isinstance(this, cw.character.Friend)){
            this.set_pos_noscale(center_noscale=(316, 142));
            // NPC表示
            cw.cwpy.cardgrp.add(this, layer=this.layer);
            cw.animation.animate_sprite(this, "deal", battlespeed=battlespeed);
            // 表示中に効果音を鳴らす
            cw.cwpy.play_sound_with(soundpath, header, subvolume=volume, loopcount=loopcount, channel=channel, fade=fade);
            cw.animation.animate_sprite(this, "zoomin", battlespeed=battlespeed);
            // カード表示
            inusecardimg = cw.cwpy.set_inusecardimg(this, header, center=true);
            cw.cwpy.draw(clip=inusecardimg.rect);
            if (cw.cwpy.setting.wait_usecard)
            {
                waitrate = cw.cwpy.setting.get_dealspeed(cw.cwpy.is_battlestatus())*2+1;
                cw.cwpy.wait_frame(waitrate, cw.cwpy.setting.can_skipanimation);
            }else{
                waitrate = cw.cwpy.setting.get_dealspeed(cw.cwpy.is_battlestatus())+1;
                cw.cwpy.wait_frame(waitrate, cw.cwpy.setting.can_skipanimation);
            }
            // カード消去
            cw.cwpy.clear_inusecardimg(this);
            // 自分が対象の時でなければNPC消去
            if (!this in targets)
            {
                cw.animation.animate_sprite(this, "zoomout", battlespeed=battlespeed);
                cw.animation.animate_sprite(this, "hide", battlespeed=battlespeed);
                cw.cwpy.cardgrp.remove(this);
            }else{
                removeafter = true;
            }
        }else{
            cw.cwpy.set_inusecardimg(this, header);
            // 効果音を鳴らす
            cw.cwpy.play_sound_with(soundpath, header, subvolume=volume, loopcount=loopcount, channel=channel, fade=fade);
            cw.animation.animate_sprite(this, "zoomin", battlespeed=battlespeed);
            if (cw.cwpy.setting.wait_usecard)
            {
                waitrate = cw.cwpy.setting.get_dealspeed(cw.cwpy.is_battlestatus());
                cw.cwpy.wait_frame(waitrate, cw.cwpy.setting.can_skipanimation);
            }
        }

        // 宿へ取り込んだ特殊文字の使用時イベントでの表示に備える
        specialchars = cw.cwpy.rsrc.specialchars;
        specialchars_is_changed = cw.cwpy.rsrc.specialchars_is_changed;
        e_mates = header.carddata.find("Property/Materials");
        can_loaded_scaledimage = header.carddata.getbool(".", "scaledimage", false);
        if (cw.cwpy.is_playingscenario() && !e_mates == null)
        {
            specialchars = specialchars.copy();
            dpath = cw.util.join_yadodir(e_mates.text);
            if (os.path.isdir(dpath))
            {
                foreach (var fname in os.listdir(dpath))
                {
                    cw.cwpy.sdata.eat_spchar(dpath, fname, can_loaded_scaledimage);
                }
            }
        }
        try
        {
            // カードイベント開始
            e = data.find("Events/Event");
            cw.event.CardEvent(e, header, this, targets).start();
        }
        finally
        {
            if (removeafter)
            {
                // NPC消去
                battlespeed = cw.cwpy.is_battlestatus();
                cw.animation.animate_sprite(this, "hide", battlespeed=battlespeed);
                cw.cwpy.cardgrp.remove(this);
            }
            // 特殊文字を元に戻す
            cw.cwpy.rsrc.specialchars = specialchars;
            cw.cwpy.rsrc.specialchars_is_changed = specialchars_is_changed;
        }
    }
    
    public UNK throwaway_card(UNK header, bool from_event=true, bool update_image=true)
    {
        // """
        // 引数のheaderのカードを破棄処理する。
        // """
        if (cw.cwpy.ydata)
        {
            cw.cwpy.trade("TRASHBOX", header=header, from_event=from_event, update_image=update_image);
        }else{
            if (header.type == "SkillCard")
            {
                index = 0;
            }else if (header.type == "ItemCard" ){
                index = 1;
            }else if (header.type == "BeastCard"){
                index = 2;
            this.cardpocket[index].remove(header);
            }
        }
    }

    //---------------------------------------------------------------------------
    //　戦闘行動関係
    //---------------------------------------------------------------------------

    public UNK action()
    {
        // """設定している戦闘行動を行う。
        // BattleEngineからのみ呼ばれる。
        // """
        UNK targets;
        UNK header;
        UNK beasts;
        bool is_hidden;
        bool inarr;
        if (this.actiondata)
        {
            // targets, header, beasts = this.actiondata
            targets = this.actiondata;
            header = this.actiondata;
            beasts = this.actiondata;

            // 召喚獣カードの使用
            if (isinstance(self, cw.sprite.card.FriendCard))
            {
                ishidden = this._vanished;
            }else{
                ishidden = this.status == "hidden";
            }

            if (this.is_alive() && !ishidden && this.status != "reversed")

            {
                for (targets_b, header_b in beasts[:])
                {
                    inarr = false;
                    for (_targets_c, header_c in this.actiondata[2])
                    {
                        if (header_c == header_b)
                        {
                            inarr = true
                            break;
                        }
                    }
                    if (!inarr)
                    {
                        // カードの効果で召喚獣カードが
                        // いなくなっている場合
                        continue; // TODO
                    }
                    this.use_card(targets_b, header_b);

                    // 戦闘勝利チェック
                    if (cw.cwpy.is_battlestatus() && cw.cwpy.battle.check_win())
                    {
                        //raise cw.battle.BattleWinError(); //TODO
                    }

                    if (isinstance(self, cw.sprite.card.FriendCard))

                    {
                        ishidden = this._vanished;
                    }else{
                        ishidden = this.status == "hidden";
                    }

                    // カードの効果で行動が変わっている可能性がある
                    if (!this.actiondata || ishidden || this.status == "reversed" || !cw.cwpy.is_battlestatus())
                    {
                        break;
                    }
                }
            // 手札カードの使用
            if (this.is_alive() && !ishidden && this.status != "reversed" && this.actiondata && cw.cwpy.is_battlestatus())
            {
                //targets, header, beasts = this.actiondata //TODO
                targets = this.actiondata;
                header = this.actiondata;
                beasts = this.actiondata;                
                if (header && this.is_active() && !ishidden && this.status != "reversed")
                {
                    this.deck.use(header);
                    this.use_card(targets, header);
                }
            }


    public void set_action(UNK target,UNK header,UNK beasts=[][:],bool auto=false) //TODO
    {
        // """
        // 戦闘行動を設定。
        // auto: 自動手札選択から設定されたかどうか。
        // """
        if (auto)
        {
            this.clear_action();
            this.actiondata = (target, header, beasts);
            this.actionautoselected = auto;
        }else{
            if (this.actiondata)
            {
                beasts = this.actiondata[2];
            }
            this.clear_action();
            this.actiondata = (target, header, beasts);
            this.actionautoselected = auto;
            cw.cwpy.play_sound("page");
            // assert cw.cwpy.pre_dialogs, "%s, %s" % (this.name, header.name) //TODO 
            if (cw.cwpy.pre_dialogs)
            {
                cw.cwpy.pre_dialogs.pop();
            }
        }
        if (cw.cwpy.battle && target && header)
        {
            // 召喚獣は個々の選択時に優先行動済みリストへ追加される
            this._add_priorityacts(target, header);
        }
        this.actionend = false;
    }

    public void _add_priorityacts(UNK target, UNK h)
    {
        if (cw.cwpy.battle && target && h)
        {
            foreach (var e in this._get_motions(h))
            {
                t = e.get("type", "");
                if (!this._is_bonusedmtype(t))
                {
                    continue;
                }
                if (t)
                {
                    cw.cwpy.battle.priorityacts.append((t, target));
                }
            }
        }
    }

    public void adjust_action()
    {
        // """
        // 現在の状態に合わせて一部戦闘行動を解除する。
        // 行動不能であれば自律的な行動は行えず、
        // 麻痺・死亡状態であれば召喚獣も動けない。
        // """
        UNK _target;
        UNK _header;
        UNK beast;

        if (this.actiondata)
        {
            if (this.is_unconscious())
            {
                this.clear_action();
            } else if (this.is_inactive(check_reversed=false)){
                // _target, _header, beasts = this.actiondata
                _target = this.actiondata;
                _header = this.actiondata;
                beasts = this.actiondata;
                this.set_action(null, null, beasts, true);
            }
        }

        if (this.is_inactive(check_reversed=false))
        {
            this.deck.throwaway();
        }
    }

    public void clear_action()
    {
        this.actiondata = null;
        this.actionautoselected = false;
        this.actionend = true;
        if (cw.cwpy.battle)
        {
            for (key, target, user in cw.cwpy.battle.priorityacts[:])
            {
                if (user == self)
                {
                    cw.cwpy.battle.priorityacts.remove((key, target, user));
                }
            }
        }
    }

    public bool is_autoselectedpenalty(UNK header=null)
    {
        // """戦闘中にペナルティカードを自動選択した状態か。
        // headerにnull以外が指定された時は、選択されたペナルティカードが
        // 指定されたheaderと一致する時のみTrueを返す。
        // """
        List<UNK> headerp; 
        if (cw.cwpy.battle && this.actiondata && this.actionautoselected)
        {
            headerp = this.actiondata[1];
            return headerp && headerp.penalty && (header == null || header == headerp);
        }
        return false;
    }

    //---------------------------------------------------------------------------
    //　判定用
    //---------------------------------------------------------------------------

    public bool decide_outcome(UNK level, UNK vocation, UNK thresholdbonus=6, UNK enhance=0, UNK subbonus=0)
    {
        // """
        // 行為判定を行う。成功ならTrue。失敗ならFalseを返す。
        // level: 判定レベル。
        // vocation: 適性データ。(身体適性名, 精神適性名)のタプル。
        // thresholdbonus: アクション元の適性値+行動力強化値。効果コンテントだと6。
        // enhance: 回避・抵抗判定の場合はボーナス値。
        // subbonus: 各種判定のサブボーナス(現在は成功率修正のみ)。
        // """
        UNK dice;
        UNK udice;
        UNK tdice;
        UNK voc;
        UNK bonus;
        UNK uvalue;
        UNK tvalue;

        dice = cw.cwpy.dice.roll(2);
        if (dice == 12)
        {
            return true;
        }else if (dice == 2){
            return false;
        }
        udice = cw.cwpy.dice.roll(2);
        tdice = dice;

        thresholdbonus = int(thresholdbonus);
        voc = self.get_vocation_val(vocation);
        bonus = int(voc + enhance);
        uvalue = cw.util.div_vocation(thresholdbonus) + level + subbonus + udice;
        tvalue = cw.util.div_vocation(bonus) + self.level + tdice;
        return uvalue <= tvalue;
    }

    public bool decide_misfire(UNK level)
    {
        // """
        // カードの不発判定を行う。成功ならtrue。失敗ならfalseを返す。
        // level: 判定レベル(カードの技能レベル)。
        // """
        UNK dice;
        UNK threshold;
        bool flag;

        dice = cw.cwpy.dice.roll(2);
        threshold = level - self.level - 1;

        if (dice == 12)
        {
            flag = true;
        }else if (dice >= threshold){
            flag = true;
        }else{
            flag = false;
        }

        return flag;
    }

    //---------------------------------------------------------------------------
    //　戦闘行動設定関連
    //---------------------------------------------------------------------------

    public UNK decide_actionorder()
    {
        // """
        // 行動順位を判定する数値をthis.actionorderに設定。
        // 敏捷度と大胆性で判定。レベル・行動力は関係なし。
        // FIXME: これによって決定される行動順はCardWirthと若干異なる
        // """
        vocation_val = int(this.get_vocation_val(("agl", "uncautious")));
        d = cw.cwpy.dice.roll(2, 6);
        this.actionorder = int((vocation_val+1) * 1.4) + d;
        return this.actionorder;
    }

    public UNK decide_action()
    {
        // """
        // 自動手札選択。
        // """
        List<UNK> beasts;
        UNK targets;
        UNK effectivetargets;
        UNK bonus;
        List<UNK> header;

        this.clear_action()
        if (this.is_dead() || !cw.cwpy.status == "ScenarioBattle")
        {
            return;//TODO
        }
        // 召喚獣カード
        beasts = [];
        for (header in this.get_pocketcards(cw.POCKET_BEAST))
        {
            if (header.is_autoselectable())
            {
                targets = header.get_targets();
                effectivetargets = header.get_targets();

                if (effectivetargets)
                {
                    // 優先度の高いターゲットが存在する場合はそちらを優先選択する
                    bonus = this._get_targetingbonus_and_targets(header, effectivetargets)
                    effectivetargets = this._get_targetingbonus_and_targets(header, effectivetargets)

                    if (!header.allrange && len(targets) > 1)
                    {
                        targets = [cw.cwpy.dice.choice(effectivetargets)];
                    }
                    beasts.append((targets, header));
                    // 優先行動済みリストへ追加する
                    this._add_priorityacts(targets, header);
                }
            }
        }
        // 行動不能時は召喚獣のみ
        if (this.is_inactive())
        {
            this.set_action(null, null, beasts, true);
            return;
        }
        // 使用するカード
        headers = [];

        for (header in this.deck.hand)
        {
            if (header.is_autoselectable())
            {
                targets = header.get_targets();
                effectivetargets = header.get_targets();

                if (effectivetargets || header.target == "None")
                {
                    if (!header.allrange)
                    {
                        targets = effectivetargets;
                    }
                    headers.append((targets, header));
                }
            }
        }
        targets = this.decide_usecard(header)
        header = this.decide_usecard(header)

        if (header && !header.allrange && len(targets) > 1)
        {
            targets = [cw.cwpy.dice.choice(targets)];
        }

        // 行動設定
        this.set_action(targets, header, beasts, true);
    }

    public UNK decide_usecard(UNK headers)
    {
        // """
        // 使用可能な手札のいずれかを自動選択する。
        // """
        // 手札交換は次の特殊処理を行う
        //  * 常に最後に判定する
        //  * 適性値を-6する
        seq = []; // 手札交換以外のカード
        exchange = []; // 手札交換
        for (t in headers)
        {
            header = t[1];
            if (header.type == "ActionCard" && header.id == 0)
            {
                exchange.append(t);
            }else{
                seq.append(t);
            }
        }
        assert len(seq)+len(exchange) == len(headers);

        // カードを選択する(手札交換以外)

        // 選択値
        // カードごとに決定し、これまでの最大値を上回れば選択
        maxd = -2147483647;
        // 選択されたカード
        selected = (null, null);
        for (i, t in enumerate(itertools.chain(seq, exchange)))
        {
            header = t[1];

            // 適性値
            vocation = (int)header.get_vocation_val();
            if (seq.Count <= i) //if (len(seq) <= i)
            {
                vocation -= 6; // 手札交換なので-6
            }else{
                vocation = max(0, vocation);
            }

            // 優先選択ボーナス
            bouns = this._get_targetingbonus_and_targets(header, t[0])
            targs = this._get_targetingbonus_and_targets(header, t[0])
            
            // 選択値を計算
            d = cw.cwpy.dice.roll();
            d = (1 + vocation); // 2 + d + bonus
            if (maxd < d)
            {
                // 選択する
                selected = (targs, header);
                maxd = d;
            }
        }
        return selected;
    }
    
    public UNK _get_motions(UNK header)    {
        if (header.type == "ActionCard" and header.id == 7)
        {
            // 逃走の場合は"VanishTarget"を"Runaway"というボーナス判定用特殊効果に置換する
            return [{"type":"Runaway"}];//TODO
        }else{
            return header.carddata.getfind("Motions").getchildren();
        }
    }

    public UNK _is_bonusedmtype(UNK mtype)
    {
        return mtype in ("Runaway", "Heal");//TODO
    }

    public UNK _get_targetingbonus_and_targets(UNK header, UNK targets)
    {
        bonus = -2147483647;
        maxbonustargs = [];
        // 最大ボーナスを取得
        motions = self._get_motions(header);
        for (motion in motions)
        {
            mtype = motion.get("type", "")
            if (not self._is_bonusedmtype(mtype))
            {
                continue;
            }
            for (targ in targets){
                b = targ.get_targetingbonus(mtype);
                if (bonus == b)
                {
                    maxbonustargs.append(targ);
                }else if (bonus < b){
                    maxbonustargs = [targ];
                    bonus = b;
                }
            }
        }
        if (bonus == -2147483647)
        {
            return 0, targets;//TODO
        }
        return bonus, targets if header.allrange else maxbonustargs;//TODO
    }

    //---------------------------------------------------------------------------
    //　状態取得用
    //---------------------------------------------------------------------------

    public UNK get_targetingbonus(UNK mtype)
    {
        // """
        // 効果のターゲットとして選ばれやすくなるボーナス値を返す。
        // 現在は"Heal"タイプに対する体力減時ボーナスと
        // "Runaway"(逃走効果に対する特殊タイプ)に対する重傷時ボーナスのみ。
        // mtype: 効果タイプ。
        // """
        bonus = 0;
        if (mtype == "Heal")
        {
            per = this.get_lifeper();
            if (50 <= per)
            {
                bonus = -1;
            }else if (33 <= per){
                bonus = 0;
            }else if (25 <= per){
                bonus = 1;
            }else if (20 <= per){
                bonus = 2;
            }else if (16 <= per){
                bonus = 3;
            }else if (14 <= per){
                bonus = 4;
            }else if (12 <= per){
                bonus = 5;
            }else if (1 <= per){
                bonus = 6 + (11 - per);
            }else{
                bonus = 100;
            }
        }else if (mtype == "Runaway"){
            per = this.get_lifeper();
            if (50 <= per)
            {
                bonus = 3;
            }else if (33 <= per){
                bonus = 4;
            }else if (25 <= per){
                bonus = 5;
            }else if (20 <= per){
                bonus = 6;
            }else if (16 <= per){
                bonus = 7;
            }else if (14 <= per){
                bonus = 8;
            }else if (12 <= per){
                bonus = 9;
            }else{
                bonus = 10 + (11 - per);
            }
        }
        if (cw.cwpy.battle)
        {
            // すでにその行動のターゲットになっている場合はボーナスを入れず、
            // ターゲット回数分をペナルティとする(選択されにくくなる)
            targeting = 0;
            foreach (s, tarr, _user in cw.cwpy.battle.priorityacts)
            {
                if (mtype == s)
                {
                    if (isinstance(tarr, cw.character.Character))
                    {
                        if (tarr == this) // TODO
                        {
                            targeting += 1;
                        }
                    }else if (this in tarr){ // TODO
                        targeting += 1;
                    }
                }
            }
            if (targeting)
            {
                bonus = min(0, bonus);
                if (mtype == "Heal")
                {
                    bonus -= targeting;
                }
            }
        }
        return bonus;
    }

    public UNK get_pocketcards(UNK index)
    {
        // """
        // 所持しているカードを返す。
        // index: カードの種類。
        // """
        return this.cardpocket[index];
    }
    public UNK get_cardpocketspace()
    {
        // """
        // 最大所持カード枚数を
        // (スキルカード, アイテムカード, 召喚獣カード)のタプルで返す
        // """
        maxskillnum = this.level / 2 + this.level % 2 + 2;
        maxskillnum = cw.util.numwrap(maxskillnum, 1, 10);
        maxbeastnum = (this.level + 2) / 4;

        if ((this.level + 2) % 4)
        {
            maxbeastnum += 1;
        }
        maxbeastnum = cw.util.numwrap(maxbeastnum, 1, 10);
        return (maxskillnum, maxskillnum, maxbeastnum);
    }

    public static int calc_lifeper(UNK life, UNK maxlife)
    {
        return (int)(100.0 * life / maxlife + 0.5);
    }

    public UNK get_lifeper()
    {
        // """
        // ライフのパーセンテージを返す。
        // """
        return Character.calc_lifeper(this.life, this.maxlife);
    }

    public UNK get_bonus(UNK vocation, bool enhance_act=true)
    {
        // """
        // 適性値と行動力強化値を合計した、行為判定用のボーナス値を返す。
        // vocation: 適性データ。(身体適性名, 精神適性名)のタプル。
        // enhance_act: 行動力修正の影響を受けるか。
        // """
        value = this.get_vocation_val(vocation);
        if (enhance_act)
        {
            value += this.get_enhance_act();
        return value;
    }

    public UNK get_vocation_val(UNK vocation)
    {
        // """
        // 適性値(身体適性値 + 精神適性値)を返す。
        // 引数のvocationは(身体適性名, 精神適性名)のタプル。
        // """
        vo = vocation;
        voc = this._voc_tbl.get(vo, None);
        if (not voc == null)
        {
            return voc;
        }
        vocation = (vocation[0].lower(), vocation[1].lower());
        physical = vocation[0];
        mental = vocation[1].replace("un", "", 1);
        physical = this.physical.get(physical);
        mental = this.mental.get(mental);

        if (vocation[1].find("un") > -1);
        {
            mental = -mental;
        }
        if ((int)(mental) != mental);
        {
            if (mental < 0)
            {
                mental += 0.5;
            }else{
                mental -= 0.5;
            }
            mental = (int)(mental);
        }
        voc = (int)(physical + mental);
        voc = cw.util.numwrap(voc, -65536, 65536);
        this._voc_tbl[vo] = voc;
        return voc;
    }

    public UNK _clear_vocationcache()
    {
        // """能力値のキャッシュをクリアする。"""
        this._voc_tbl = {};
    }

    public UNK get_enhance_act()
    {
        // """
        // 行動力強化値を返す。行動力は効果コンテントによる強化値だけ。
        // """
        return cw.util.numwrap(this.enhance_act, -10, 10);
    }

    public UNK get_enhance_def()
    {
        // """
        // 初期・状態・カードによる防御力修正の計算結果を返す。
        // 単体で+10の修正がない場合は、合計値が+10を越えていても+9を返す。
        // """
        return this._get_enhance_impl("defense", this.enhance_def, 2);
    }

    public UNK get_enhance_res()
    {
        // """
        // 初期・状態・カードによる抵抗力修正の計算結果を返す。
        // """
        return this._get_enhance_impl("resist", this.enhance_res, 1);
    }

    public UNK get_enhance_avo()
    {
        // """
        // 初期・状態・カードによる回避力修正の計算結果を返す。
        // """
        return this._get_enhance_impl("avoid", this.enhance_avo, 0);
    }

    public UNK _calc_enhancevalue(UNK header, UNK value)
    {
        // """使用・所持ボーナス値に適性による補正を加える。
        // BUG: CardWirthではアイテムの所持ボーナスに限り
        //       最低適性(level=0)の時に補正係数が50%となるが、
        //      それ以外の全てのパターンではそのような計算が
        //      行われないため、バグと思われる
        // """
        if (value <= 0)
        {
            return value;
        }
        level = header.get_vocation_level(UNK enhance_act=false);
        if (level <= 2)
        {
            value2 = cw.util.numwrap(value, -10, 10);
        }else{
            value2 = cw.util.numwrap(value * 150 / 100, -10, 10);
        }
        return value2;
    }
    
//    def _get_enhance_impl(self, name, initvalue, enhindex):
//        """
//        現在かけられている全ての能力修正値の合計を返す(ただし単純な加算ではない)。
//        デフォルト修正値 + 状態修正値 + カード所持修正値 + カード使用修正値。
//        ただしカードの修正値は適性による補正を受ける。
//        """
//        val1 = int(self.enhance.get(name))
//        val1 = cw.util.numwrap(val1, -10, 10)
//        val2 = int(initvalue)
//        val2 = cw.util.numwrap(val2, -10, 10)
//        seq = [val1, val2]
//        pvals = []
//        def add_pval(val):
//            if 0 < val and val < 10:
//                pvals.append(int(val))
//
//        def addval(header, val, using=False):
//            if name == "defense":
//                val = int(val)
//                val2 = val
//                if header.type == "SkillCard":
//                    # 特殊技能使用
//                    assert using
//                    level = header.get_vocation_level(self, enhance_act=False)
//                    if 2 <= level:
//                        val = val * 120 // 100
//                    add_pval(val)
//                    val = val2
//                elif header.type == "ItemCard" and using:
//                    # アイテム使用
//                    add_pval(val)
//                elif header.type == "ItemCard":
//                    # アイテム所持
//                    level = header.get_vocation_level(self, enhance_act=False)
//                    if val < 0:
//                        if 3 <= level:
//                            val = val * 80 // 100
//                        elif level <= 0:
//                            val = val * 150 // 100
//                        add_pval(val)
//                    elif 0 < val:
//                        if level <= 0:
//                            val = val * 50 // 100
//                        elif level <= 1:
//                            val = val * 80 // 100
//                        add_pval(val)
//                elif header.type == "BeastCard":
//                    # 召喚獣所持
//                    add_pval(val)
//                else:
//                    assert header.type == "ActionCard"
//                    add_pval(val)
//            else:
//                val = self._calc_enhancevalue(header, val)
//                add_pval(val)
//            val = int(val)
//            seq.append(val)
//
//        if self.actiondata and self.actiondata[1]:
//            header = self.actiondata[1]
//        elif self.deck and self.deck.get_used():
//            header = self.deck.get_used()
//        else:
//            header = None
//
//        if header:
//            val4 = header.get_enhance_val_used()[enhindex]
//            addval(header, val4, True)
//
//        for header in itertools.chain(self.get_pocketcards(cw.POCKET_BEAST),
//                                      self.get_pocketcards(cw.POCKET_ITEM)):
//            val3 = header.get_enhance_val()[enhindex]
//            addval(header, val3)
//
//        add_pval(val1)
//        add_pval(val2)
//
//        a = 0
//        b = 0
//        ac = 0
//        bc = 0
//        max10 = 0
//        max10counter = 0
//        maxval = 0
//        minval = 0
//        for val in seq:
//            if val < 0:
//                if a == 0:
//                    a = (10 + val)
//                else:
//                    a *= (10 + val)
//                ac += 1
//                minval = min(minval, val)
//                max10counter += -val//6 + 1
//            elif 0 < val:
//                if b == 0:
//                    b = (10 - val)
//                else:
//                    b *= (10 - val)
//                bc += 1
//                max10 += val // 10
//                maxval = max(maxval, val)
//        if ac:
//            a /= math.pow(10, ac-1)
//            a = 10 - a
//            a = max(-minval, a)
//        if bc:
//            b /= math.pow(10, bc-1)
//            b = 10 - b
//            b = max(maxval, b)
//
//        pvalr = 100
//        for pval in reversed(pvals):
//            pvalr *= 10
//            pvalr *= 10-pval
//            pvalr //= 100
//
//        if pvalr < 1:
//            # 防御修正でn[1],n[2],n[3],...,n[N]の値がある時、
//            # (1-n[N]/10)*(1-n[N-1]/10)*...,(1-n[1]/10)の結果が
//            # 0.01未満になれば+10効果を得られる(計算途中の誤差は切り捨て)。
//            # ただし適性による変動がある
//            max10 += 1
//
//        if max10 < 3:
//            # 防御修正で+10があると完全にダメージが無くなるが、
//            # -1～5で1回、-6以上で2回分、+10効果を打ち消すことができる
//            # ただし+30以上は無効化不可
//            max10 -= max10counter
//
//        value = int(b) - int(a)
//        if name == "defense":
//            if 0 < max10:
//                value = 10
//            else:
//                # 防御ボーナスは単体の+10がない限り最大で+9になる
//                value = cw.util.numwrap(value, -10, 9)
//        else:
//            value = cw.util.numwrap(value, -10, 10)
//
//        return value
//
//    #---------------------------------------------------------------------------
//    #　クーポン関連
//    #---------------------------------------------------------------------------
//
//    @synclock(_couponlock)
//    def get_coupons(self):
//        """
//        所有クーポンをセット型で返す。
//        """
//        return self._get_coupons()
//    def _get_coupons(self):
//        return set(self.coupons.iterkeys())
//
//    @synclock(_couponlock)
//    def get_couponvalue(self, name, raiseerror=True):
//        """
//        クーポンの値を返す。
//        """
//        return self._get_couponvalue(name, raiseerror)
//
//    def _get_couponvalue(self, name, raiseerror=True):
//        if raiseerror:
//            return self.coupons[name][0]
//        else:
//            data = self.coupons.get(name, None)
//            if data:
//                return data[0]
//            else:
//                return None
//
//    @synclock(_couponlock)
//    def has_coupon(self, coupon):
//        """
//        引数のクーポンを所持しているかbool値で返す。
//        """
//        return self._has_coupon(coupon)
//
//    def _has_coupon(self, coupon):
//        return coupon in self.coupons
//
//    @synclock(_couponlock)
//    def get_couponsvalue(self):
//        return self._get_couponsvalue()
//
//    def _get_couponsvalue(self):
//        """
//        全ての所持クーポンの点数を合計した値を返す。
//        """
//        cnt = 0
//
//        for coupon, data in self.coupons.iteritems():
//            if coupon and not coupon[0] in (u"＠", u"：", u"；"):
//                value = data[0]
//                cnt += value
//
//        return cnt
//
//    @synclock(_couponlock)
//    def get_specialcoupons(self):
//        """
//        "＠"で始まる特殊クーポンの
//        辞書(key=クーポン名, value=クーポン得点)を返す。
//        """
//        return self._get_specialcoupons()
//
//    def _get_specialcoupons(self):
//        d = {}
//
//        for coupon, data in self.coupons.iteritems():
//            if coupon and coupon.startswith(u"＠"):
//                value = data[0]
//                d[coupon] = value
//
//        return d
//
//    @synclock(_couponlock)
//    def replace_allcoupons(self, seq, syscoupons={}.copy()):
//        """システムクーポン以外の全てのクーポンを
//        listの内容に入れ替える。
//        所持クーポンが変化したらTrueを返す。
//        seq: クーポン情報のタプル(name, value)のリスト。
//        syscoupons: このコレクション内にあるクーポンは
//                    システムクーポンとして処理対象外にする
//        """
//        old_coupons = {}
//        for name, (value, e) in self.coupons.iteritems():
//            old_coupons[name] = value
//        revcoupon_old = False
//        revcoupon_new = False
//        # システムクーポン以外を一旦除去
//        for name in self._get_coupons():
//            if syscoupons is None or not (name.startswith(u"＠") or name in syscoupons):
//                self._remove_coupon(name, False)
//            revcoupon_old |= (name == u"：Ｒ")
//
//        sexcoupons = set(cw.cwpy.setting.sexcoupons)
//        periodcoupons = set(cw.cwpy.setting.periodcoupons)
//        naturecoupons = set(cw.cwpy.setting.naturecoupons)
//
//        # クーポン追加
//        for coupon in seq:
//            name = coupon[0]
//            if name in sexcoupons:
//                old = self._get_sex()
//                if old:
//                    self._remove_coupon(old)
//            if name in periodcoupons:
//                old = self._get_age()
//                if old:
//                    self._remove_coupon(old)
//            if name in naturecoupons:
//                old = self._get_talent()
//                if old:
//                    self._remove_coupon(old)
//
//            self._set_coupon(name, coupon[1], False)
//            revcoupon_new |= (name == u"：Ｒ")
//
//        # 隠蔽クーポン
//        if revcoupon_old <> revcoupon_new:
//            self.reversed = revcoupon_old
//            if self.status == "hidden":
//                self.reverse()
//            else:
//                cw.animation.animate_sprite(self, "reverse")
//
//        new_coupons = {}
//        for name, (value, e) in self.coupons.iteritems():
//            new_coupons[name] = value
//        return new_coupons <> old_coupons
//
//    @synclock(_couponlock)
//    def get_sex(self):
//        return self._get_sex()
//
//    def _get_sex(self):
//        for coupon in cw.cwpy.setting.sexcoupons:
//            if coupon in self.coupons:
//                return coupon
//
//        return cw.cwpy.setting.sexcoupons[0]
//
//    @synclock(_couponlock)
//    def set_sex(self, sex):
//        self._set_sex(sex)
//
//    def _set_sex(self, sex):
//        if cw.cwpy.ydata:
//            cw.cwpy.ydata.changed()
//        old = self._get_sex()
//        if old:
//            self._remove_coupon(old)
//        self._set_coupon(sex, 0)
//
//    @synclock(_couponlock)
//    def has_sex(self):
//        return self._has_sex()
//
//    def _has_sex(self):
//        for coupon in cw.cwpy.setting.sexcoupons:
//            if coupon in self.coupons:
//                return True
//
//        return False
//
//    @synclock(_couponlock)
//    def get_age(self):
//        return self._get_age()
//
//    def _get_age(self):
//        for coupon in cw.cwpy.setting.periodcoupons:
//            if coupon in self.coupons:
//                return coupon
//
//        return cw.cwpy.setting.periodcoupons[0]
//
//    @synclock(_couponlock)
//    def set_age(self, age):
//        self._set_age(age)
//
//    def _set_age(self, age):
//        if cw.cwpy.ydata:
//            cw.cwpy.ydata.changed()
//        old = self._get_age()
//        if old:
//            self._remove_coupon(old)
//        self._set_coupon(age, 0)
//
//    @synclock(_couponlock)
//    def has_age(self):
//        return self._has_age()
//
//    def _has_age(self):
//        for coupon in cw.cwpy.setting.periodcoupons:
//            if coupon in self.coupons:
//                return True
//
//        return False
//
//    @synclock(_couponlock)
//    def get_talent(self):
//        return self._get_talent()
//
//    def _get_talent(self):
//        for coupon in cw.cwpy.setting.naturecoupons:
//            if coupon in self.coupons:
//                return coupon
//
//        return cw.cwpy.setting.naturecoupons[0]
//
//    @synclock(_couponlock)
//    def set_talent(self, talent):
//        self._set_talent(talent)
//
//    def _set_talent(self, talent):
//        if cw.cwpy.ydata:
//            cw.cwpy.ydata.changed()
//        old = self._get_talent()
//        if old:
//            self._remove_coupon(old)
//        self._set_coupon(talent, 0)
//
//    @synclock(_couponlock)
//    def has_talent(self):
//        return self._has_talent()
//
//    def _has_talent(self):
//        for coupon in cw.cwpy.setting.naturecoupons:
//            if coupon in self.coupons:
//                return True
//
//        return False
//
//    @synclock(_couponlock)
//    def get_makings(self):
//        return self._get_makings()
//
//    def _get_makings(self):
//        """
//        所持する特徴クーポンをセット型で返す。
//        """
//        makings = set()
//        for making in cw.cwpy.setting.makingcoupons:
//            if making in self.coupons:
//                makings.add(making)
//        return makings
//
//    @synclock(_couponlock)
//    def set_makings(self, makings):
//        return self._set_makings(makings)
//
//    def _set_makings(self, makings):
//        if cw.cwpy.ydata:
//            cw.cwpy.ydata.changed()
//        for coupon in cw.cwpy.setting.makingcoupons:
//            if coupon in self.coupons:
//                self._remove_coupon(coupon)
//        for coupon in makings:
//            self._set_coupon(coupon, 0)
//
//    @synclock(_couponlock)
//    def set_race(self, race):
//        self._set_race(race)
//    def _set_race(self, race):
//        old = self._get_race()
//        if race == old:
//            return
//        if not isinstance(old, cw.header.UnknownRaceHeader):
//            self._remove_coupon(u"＠Ｒ" + old.name)
//        if not isinstance(race, cw.header.UnknownRaceHeader):
//            self._set_coupon(u"＠Ｒ" + race.name, 0)
//
//    @synclock(_couponlock)
//    def get_race(self):
//        return self._get_race()
//
//    def _get_race(self):
//        for race in cw.cwpy.setting.races:
//            if self._has_coupon(u"＠Ｒ" + race.name):
//                return race
//        return cw.cwpy.setting.unknown_race
//
//    @synclock(_couponlock)
//    def count_timedcoupon(self, value=-1):
//        """
//        時限クーポンの点数を減らす。
//        value: 減らす数。
//        """
//        if self.timedcoupons:
//            if cw.cwpy.ydata:
//                cw.cwpy.ydata.changed()
//            self.data.is_edited = True
//
//            for coupon in list(self.timedcoupons):
//                oldvalue, e = self.coupons[coupon]
//                if oldvalue == 0:
//                    continue
//
//                n = oldvalue + value
//                n = cw.util.numwrap(n, 0, 999)
//
//                if n > 0:
//                    e.set("value", str(n))
//                    self.coupons[coupon] = n, e
//                else:
//                    self._remove_coupon(coupon)
//
//    @synclock(_couponlock)
//    def set_coupon(self, name, value):
//        """
//        クーポンを付与する。同名のクーポンがあったら上書き。
//        時限クーポン("："or"；"で始まるクーポン)はtimedcouponsに登録する。
//        name: クーポン名。
//        value: クーポン点数。
//        """
//        self._set_coupon(name, value, True)
//
//    def _set_coupon(self, name, value, update=True):
//        if cw.cwpy.ydata:
//            cw.cwpy.ydata.changed()
//        value = int(value)
//        value = cw.util.numwrap(value, -999, 999)
//        removed = self._remove_coupon(name, False)
//        e = self.data.make_element("Coupon", name, {"value" : str(value)})
//        self.data.append("Property/Coupons", e)
//        self.coupons[name] = value, e
//
//        # 時限クーポン
//        if name.startswith(u"：") or name.startswith(u"；"):
//            self.timedcoupons.add(name)
//
//        # 隠蔽クーポン
//        if name == u"：Ｒ" and not self.is_reversed():
//            if update and not removed:
//                if self.status == "hidden":
//                    self.reverse()
//                else:
//                    cw.animation.animate_sprite(self, "reverse")
//            self.reversed = True
//
//        if not removed:
//            # 効果対象の変更(Wsn.2)
//            effectevent = cw.cwpy.event.get_effectevent()
//            if effectevent and name == u"＠効果対象":
//                effectevent.add_target(self)
//
//        # 隠蔽クーポンがあるため
//        self.adjust_action()
//
//    @synclock(_couponlock)
//    def get_timedcoupons(self):
//        """
//        時限クーポンのデータをまとめたsetを返す。
//        """
//        s = set()
//
//        for coupon in self.coupons.iterkeys():
//            if coupon.startswith(u"：") or coupon.startswith(u"；"):
//                s.add(coupon)
//
//        return s
//
//    @synclock(_couponlock)
//    def remove_coupon(self, name):
//        """
//        同じ名前のクーポンを全て剥奪する。
//        name: クーポン名。
//        """
//        return self._remove_coupon(name, True)
//
//    def _remove_coupon(self, name, update=True):
//        if not name in self.coupons:
//            return False
//        if cw.cwpy.ydata:
//            cw.cwpy.ydata.changed()
//
//        _value, e = self.coupons[name]
//        self.data.remove("Property/Coupons", e)
//        del self.coupons[name]
//
//        # 時限クーポン
//        if name in self.timedcoupons:
//            self.timedcoupons.remove(name)
//
//        # 隠蔽クーポン
//        if name == u"：Ｒ" and self.is_reversed():
//            if update:
//                if self.status == "hidden":
//                    self.reverse()
//                else:
//                    cw.animation.animate_sprite(self, "reverse")
//            self.reversed = False
//
//        # 効果対象の変更(Wsn.2)
//        effectevent = cw.cwpy.event.get_effectevent()
//        if effectevent and name == u"＠効果対象":
//            effectevent.remove_target(self)
//
//        return True
//
//    @synclock(_couponlock)
//    def remove_timedcoupons(self, battleonly=False):
//        """
//        時限クーポンを削除する。イメージは更新しない。
//        battleonly: Trueの場合は"；"の時限クーポンのみ削除。
//        """
//        for name in set(self.timedcoupons):
//            if not battleonly or name.startswith(u"；"):
//                self._remove_coupon(name, False)
//
//    @synclock(_couponlock)
//    def remove_numbercoupon(self):
//        """
//        "＿１"等の番号クーポンを削除。
//        """
//        # u"＠ＭＰ３"はCardWirth 1.29以降で配布されるクーポン
//        names = [cw.cwpy.msgs["number_1_coupon"], u"＿１", u"＿２", u"＿３", u"＿４", u"＿５", u"＿６", u"＠ＭＰ３"]
//
//        for name in names:
//            self._remove_coupon(name)
//
//    #---------------------------------------------------------------------------
//    #　レベル変更用
//    #---------------------------------------------------------------------------
//
//    @synclock(_couponlock)
//    def get_limitlevel(self):
//        """レベルの調節範囲の最大値を返す。"""
//        return self._get_limitlevel()
//
//    def _get_limitlevel(self):
//        l = self._get_couponvalue(u"＠レベル原点", raiseerror=False)
//        if not l is None:
//            return max(self.level, l)
//        else:
//            return self.level
//
//    @synclock(_couponlock)
//    def check_level(self):
//        coupons = self._get_specialcoupons()
//        level = coupons[u"＠レベル原点"]
//
//        limit = self._get_levelmax(coupons)
//        if not u"＠レベル上限" in coupons:
//            self._set_coupon(u"＠レベル上限", limit)
//
//        # 解の公式で現在の経験点で到達できるレベルを算出
//        cnt = max(1, self._get_couponsvalue())
//        olevel = int((-1 + math.sqrt(1 + 4 * cnt)) / 2.0) + 1
//        olevel = min(limit, olevel)
//
//        return olevel - level
//
//    @synclock(_couponlock)
//    def get_levelmax(self):
//        coupons = self._get_specialcoupons()
//        return self._get_levelmax(coupons)
//
//    def _get_levelmax(self, coupons):
//        if u"＠レベル上限" in coupons:
//            limit = coupons[u"＠レベル上限"]
//        elif u"＠本来の上限" in coupons:
//            limit = coupons[u"＠本来の上限"]
//            self._set_coupon(u"＠レベル上限", limit)
//        else:
//            limit = 10
//        return limit
//
//    @synclock(_couponlock)
//    def set_level(self, value, regulate=False, debugedit=False, backpack_party=None, revert_cardpocket=True):
//        """レベルを設定する。
//        regulate: レベルを調節する場合はTrue。
//        backpack_party: レベルが下がって手札を持ちきれなくなった際、
//                        このパーティの荷物袋へ入れる。
//                        Noneの場合はアクティブなパーティの荷物袋か
//                        カード置場へ入る。
//        """
//        # 調節前のレベル
//        limit = self._get_limitlevel()
//        if regulate:
//            value = min(value, limit)
//
//        # レベル
//        uplevel = value - self.level
//        if uplevel == 0:
//            return
//
//        if cw.cwpy.ydata:
//            cw.cwpy.ydata.changed()
//
//        vit = max(1, int(self.physical.get("vit")))
//        minval = max(1, int(self.physical.get("min")))
//
//        coeff = self.data.getfloat("Property/Life", "coefficient", 0.0)
//        if coeff <= 0.0:
//            maxlife = calc_maxlife(vit, minval, self.level)
//            if int(maxlife) == self.maxlife:
//                coeff = 1
//            else:
//                # 最大HP10でレベル10のキャラクタのレベルを9に下げたら
//                # 最大HPが90に増えてしまった、というような問題を
//                # 避けるため、計算上の体力と実際の最大体力が食い違う
//                # 場合は計算用係数を付与する
//                coeff = float(self.maxlife) / int(maxlife)
//                self.data.edit("Property/Life", str(coeff), "coefficient")
//
//        self.level = value
//        self.data.edit("Property/Level", str(self.level))
//        # 最大HPとHP
//        maxlife = calc_maxlife(vit, minval, self.level)
//        if coeff <> 1:
//            maxlife = round(maxlife * coeff)
//        maxlife = int(max(1, maxlife))
//        self.maxlife += maxlife - self.maxlife
//        self.data.edit("Property/Life", str(self.maxlife), "max")
//        self.set_life(self.maxlife)
//        # 技能の使用回数
//        for header in self.cardpocket[cw.POCKET_SKILL]:
//            header.get_uselimit(reset=True)
//
//        if not regulate:
//            # レベル原点・EPクーポン操作
//            for e in self.data.find("Property/Coupons"):
//                if not e.text:
//                    continue
//
//                if e.text == u"＠レベル原点":
//                    e.attrib["value"] = str(self.level)
//                    self.coupons[e.text] = self.level, e
//                elif e.text == u"＠ＥＰ":
//                    value = e.getint(".", "value", 0) + (value - limit) * 10
//                    e.attrib["value"] = str(value)
//                    self.coupons[e.text] = value, e
//
//        if uplevel < 0:
//            # 所持可能上限を越えたカードを、荷物袋ないしカード置き場へ移動
//            if backpack_party or isinstance(self, cw.sprite.card.PlayerCard):
//                targettype = "BACKPACK"
//            else:
//                targettype = "STOREHOUSE"
//            targettype_original = targettype
//            for index in range(3):
//                n = len(self.cardpocket[index])
//                maxn = self.get_cardpocketspace()[index]
//                while n > maxn:
//                    header = self.cardpocket[index][-1]
//                    if index == cw.POCKET_BEAST and not header.attachment:
//                        targettype = "TRASHBOX"
//                    else:
//                        targettype = targettype_original
//                    if regulate and targettype <> "TRASHBOX":
//                        self.add_cardpocketmemory(header)
//                    cw.cwpy.trade(targettype=targettype, header=header, from_event=True, party=backpack_party, sort=False)
//                    n -= 1
//            if targettype_original == "BACKPACK":
//                cw.cwpy.ydata.party.sort_backpack()
//            elif targettype_original == "STOREHOUSE":
//                cw.cwpy.ydata.sort_storehouse()
//            for header in self.cardpocket[cw.POCKET_SKILL]:
//                header.get_uselimit(reset=True)
//        elif 0 < uplevel and revert_cardpocket:
//            # レベル調節で手放したカードを戻す
//            self.revert_cardpocket(backpack_party)
//
//    def add_cardpocketmemory(self, header):
//        """レベル調節前に所持していたカードを記憶する。"""
//        memories = self.data.find("./CardMemories")
//        if memories is None:
//            memories = cw.data.make_element("CardMemories")
//            self.data.append(".", memories)
//        e = cw.data.make_element("CardMemory")
//        e.append(cw.data.make_element("Type", header.type))
//        e.append(cw.data.make_element("Name", header.name))
//        e.append(cw.data.make_element("Description", header.desc))
//        e.append(cw.data.make_element("Scenario", header.scenario))
//        e.append(cw.data.make_element("Author", header.author))
//        if type <> "BeastCard":
//            e.append(cw.data.make_element("Hold", str(header.hold)))
//        if header.type <> "SkillCard":
//            e.append(cw.data.make_element("UseLimit", str(header.uselimit)))
//        memories.append(e)
//
//    def revert_cardpocket(self, backpack_party=None):
//        """記憶していたカードを検索し、
//        見つかったら再び所持する。"""
//        if not cw.cwpy.setting.revert_cardpocket:
//            return
//
//        if not backpack_party:
//            backpack_party = cw.cwpy.ydata.party
//
//        seq = []
//        if backpack_party:
//            seq.extend(backpack_party.backpack)
//            if not backpack_party.is_adventuring():
//                seq.extend(cw.cwpy.ydata.storehouse)
//        else:
//            seq.extend(cw.cwpy.ydata.storehouse)
//
//        maxn = self.get_cardpocketspace()
//        n = [
//             len(self.get_pocketcards(cw.POCKET_SKILL)),
//             len(self.get_pocketcards(cw.POCKET_ITEM)),
//             len(self.get_pocketcards(cw.POCKET_BEAST)),
//        ]
//        for e in reversed(self.data.getfind("./CardMemories", False)[:]):
//            cardtype = e.gettext("./Type")
//            name = e.gettext("./Name", "")
//            desc = e.gettext("./Description", "")
//            scenario = e.gettext("./Scenario", "")
//            author = e.gettext("./Author", "")
//            if cardtype == "SkillCard":
//                index = cw.POCKET_SKILL
//                uselimit = -1
//            elif cardtype == "ItemCard":
//                index = cw.POCKET_ITEM
//                uselimit = e.getint("./UseLimit", -1)
//            elif cardtype == "BeastCard":
//                index = cw.POCKET_BEAST
//                uselimit = e.getint("./UseLimit", -1)
//
//            if n[index] < maxn[index]:
//                for header in seq:
//                    if header.type == cardtype and\
//                            header.name == name and\
//                            header.desc == desc and\
//                            header.scenario == scenario and\
//                            header.author == author and\
//                            (uselimit == -1 or uselimit == header.uselimit):
//                        n[index] += 1
//                        cw.cwpy.trade("PLAYERCARD", target=self, header=header, from_event=True, party=backpack_party)
//                        seq.remove(header)
//                        if cardtype <> "BeastCard":
//                            hold = e.getbool("./Hold", False)
//                            header.set_hold(hold)
//                        break
//                # 記憶に残すのは持ちきれなかった場合のみ
//                # 持ちきれる場合はカードが見つからなくても
//                # 記憶から除去する
//                self.data.remove("./CardMemories", e)
//
//    #---------------------------------------------------------------------------
//    #　状態変更用
//    #---------------------------------------------------------------------------
//
//    def set_unconsciousstatus(self, clearbeast=True):
//        """
//        意識不明に伴う状態回復。
//        強化値もすべて0、付帯召喚以外の召喚獣カードも消去。
//        毒と麻痺は残る。
//        """
//        self.set_mentality("Normal", 0)
//        self.set_bind(0)
//        self.set_silence(0)
//        self.set_faceup(0)
//        self.set_antimagic(0)
//        self.set_enhance_act(0, 0)
//        self.set_enhance_avo(0, 0)
//        self.set_enhance_res(0, 0)
//        self.set_enhance_def(0, 0)
//        if clearbeast:
//            self.set_beast(vanish=True)
//
//    def set_fullrecovery(self, decideaction=False):
//        """
//        完全回復処理。HP＆精神力＆状態異常回復。
//        強化値もすべて0、付帯召喚以外の召喚獣カードも消去。
//        """
//        self.set_life(self.maxlife)
//        self.set_paralyze(-40)
//        self.set_poison(-40)
//        self.set_mentality("Normal", 0)
//        self.set_bind(0)
//        self.set_silence(0)
//        self.set_faceup(0)
//        self.set_antimagic(0)
//        self.set_enhance_act(0, 0)
//        self.set_enhance_avo(0, 0)
//        self.set_enhance_res(0, 0)
//        self.set_enhance_def(0, 0)
//        self.set_skillpower()
//        self.set_beast(vanish=True)
//
//        # 行動を再選択する
//        if decideaction and cw.cwpy.is_battlestatus() and cw.cwpy.battle.is_ready() and self.is_active():
//            self.deck.set(self)
//            self.decide_action()
//
//    def set_life(self, value):
//        """
//        現在ライフに引数nの値を足す(nが負だと引き算でダメージ)。
//        """
//        if cw.cwpy.ydata:
//            cw.cwpy.ydata.changed()
//        oldlife = self.life
//        self.life += value
//        self.life = cw.util.numwrap(self.life, 0, self.maxlife)
//        self.data.edit("Property/Life", str(int(self.life)))
//        self.adjust_action()
//        if self.is_unconscious():
//            self.set_unconsciousstatus()
//        return self.life - oldlife
//
//    def set_paralyze(self, value):
//        """
//        麻痺値を操作する。
//        麻痺値は0～40の範囲を越えない。
//        """
//        if cw.cwpy.ydata:
//            cw.cwpy.ydata.changed()
//        old = self.paralyze
//        self.paralyze += value
//        self.paralyze = cw.util.numwrap(self.paralyze, 0, 40)
//        if 0 < self.paralyze:
//            self.set_mentality("Normal", 0)
//        self.data.edit("Property/Status/Paralyze", str(self.paralyze))
//        self.adjust_action()
//        return self.paralyze - old
//
//    def set_poison(self, value):
//        """
//        中毒値を操作する。
//        中毒値は0～40の範囲を越えない。
//        """
//        if cw.cwpy.ydata:
//            cw.cwpy.ydata.changed()
//        old = self.poison
//        self.poison += value
//        self.poison = cw.util.numwrap(self.poison, 0, 40)
//        self.data.edit("Property/Status/Poison", str(self.poison))
//        return self.poison - old
//
//    def set_mentality(self, name, value, overwrite=True):
//        """
//        精神状態とその継続ラウンド数を操作する。
//        継続ラウンド数の範囲は0～999を越えない。
//        """
//        if cw.cwpy.ydata:
//            cw.cwpy.ydata.changed()
//        if self.is_unconscious() or self.is_paralyze():
//            name = "Normal"
//            value = 0
//        value = cw.util.numwrap(value, 0, 999)
//        if name == "Normal":
//            value = 0
//        elif value == 0:
//            name = "Normal"
//
//        if not overwrite and name == self.mentality and name <> "Normal":
//            # 長い方の効果時間を優先
//            self.mentality_dur = max(self.mentality_dur, value)
//        else:
//            self.mentality = name
//            self.mentality_dur = value
//
//        path = "Property/Status/Mentality"
//        self.data.edit(path, self.mentality)
//        self.data.edit(path, str(self.mentality_dur), "duration")
//        self.adjust_action()
//
//    def set_bind(self, value, overwrite=True):
//        """
//        束縛状態の継続ラウンド数を操作する。
//        継続ラウンド数の範囲は0～999を越えない。
//        """
//        if cw.cwpy.ydata:
//            cw.cwpy.ydata.changed()
//        if self.is_unconscious():
//            value = 0
//        if overwrite:
//            self.bind = value
//        else:
//            self.bind = max(self.bind, value)
//        self.bind = cw.util.numwrap(self.bind, 0, 999)
//        self.data.edit("Property/Status/Bind", str(self.bind), "duration")
//        self.adjust_action()
//
//    def set_silence(self, value, overwrite=True):
//        """
//        沈黙状態の継続ラウンド数を操作する。
//        継続ラウンド数の範囲は0～999を越えない。
//        """
//        if cw.cwpy.ydata:
//            cw.cwpy.ydata.changed()
//        if self.is_unconscious():
//            value = 0
//        if overwrite:
//            self.silence = value
//        else:
//            self.silence = max(self.silence, value)
//        self.silence = cw.util.numwrap(self.silence, 0, 999)
//        self.data.edit("Property/Status/Silence", str(self.silence), "duration")
//
//    def set_faceup(self, value, overwrite=True):
//        """
//        暴露状態の継続ラウンド数を操作する。
//        継続ラウンド数の範囲は0～999を越えない。
//        """
//        if cw.cwpy.ydata:
//            cw.cwpy.ydata.changed()
//        if self.is_unconscious():
//            value = 0
//        if overwrite:
//            self.faceup = value
//        else:
//            self.faceup = max(self.faceup, value)
//        self.faceup = cw.util.numwrap(self.faceup, 0, 999)
//        self.data.edit("Property/Status/FaceUp", str(self.faceup), "duration")
//
//    def set_antimagic(self, value, overwrite=True):
//        """
//        魔法無効状態の継続ラウンド数を操作する。
//        継続ラウンド数の範囲は0～999を越えない。
//        """
//        if cw.cwpy.ydata:
//            cw.cwpy.ydata.changed()
//        if self.is_unconscious():
//            value = 0
//        if overwrite:
//            self.antimagic = value
//        else:
//            self.antimagic = max(self.antimagic, value)
//        self.antimagic = cw.util.numwrap(self.antimagic, 0, 999)
//        self.data.edit("Property/Status/AntiMagic", str(self.antimagic), "duration")
//
//    def set_vanish(self, battlespeed=False):
//        """
//        対象消去を行う。
//        """
//        if isinstance(self, cw.character.Friend):
//            # 1.50までは同行NPCに対象消去は効かない
//            return
//        if not self.is_vanished():
//            if cw.cwpy.ydata:
//                cw.cwpy.ydata.changed()
//            self._vanished = True
//            if isinstance(self, cw.character.Player):
//                cw.animation.animate_sprite(self, "vanish", battlespeed=battlespeed)
//                if cw.cwpy.sct.enable_vanishmembercancellation(cw.cwpy.sdata.get_versionhint(frompos=cw.HINT_SCENARIO)) or\
//                   cw.cwpy.sct.enable_vanishmembercancellation(cw.cwpy.sdata.get_versionhint(frompos=cw.HINT_AREA)):
//                    cw.cwpy.ydata.party.vanished_pcards.append(self)
//                else:
//                    self.commit_vanish()
//            else:
//                cw.animation.animate_sprite(self, "delete", battlespeed=battlespeed)
//                self.commit_vanish()
//            cw.cwpy.vanished_card(self)
//
//    def cancel_vanish(self):
//        """対象消去をキャンセルする。
//        表示処理は行わないため、呼び出し後に行う必要がある。
//        """
//        if not self.is_vanished():
//            return
//        if cw.cwpy.ydata:
//            cw.cwpy.ydata.changed()
//
//        assert not cw.cwpy.cardgrp.has(self)
//        assert self.data in cw.cwpy.ydata.party.members
//
//        self._vanished = False
//        cw.cwpy.cardgrp.add(self, layer=self.layer)
//        cw.cwpy.pcards.insert(cw.cwpy.ydata.party.members.index(self.data), self)
//
//    def commit_vanish(self):
//        if not self.is_vanished():
//            return
//        if cw.cwpy.ydata:
//            cw.cwpy.ydata.changed()
//
//        if isinstance(self, cw.character.Player):
//            # PCの場合、プレミアカードを荷物袋へ移動する
//            for pocket in self.cardpocket:
//                for card in pocket[:]:
//                    if card.premium == "Premium":
//                        cw.cwpy.trade("BACKPACK", header=card, from_event=True, sort=False)
//
//            index = cw.cwpy.ydata.party.members.index(self.data)
//            for i in xrange(index, len(cw.cwpy.ydata.party.members)):
//                pi = i + 1
//                cw.cwpy.file_updates.update(cw.cwpy.update_pcimage(pi, deal=False))
//
//            for bgtype, d in cw.cwpy.background.bgs:
//                if bgtype == cw.sprite.background.BG_PC:
//                    pcnumber = d[0]
//                    if index + 1 <= pcnumber:
//                        cw.cwpy.file_updates_bg = True
//                        break
//                elif bgtype == cw.sprite.background.BG_TEXT:
//                    namelist = d[1]
//                    for item in namelist:
//                        if item.data is self:
//                            # テキストセルに表示中の名前
//                            # 対象消去された場合は最後に表示された文字列に固定する
//                            item.data = None
//
//            cw.cwpy.ydata.party.remove(self)
//
//        self.lost()
//
//    def set_enhance_act(self, value, duration):
//        """
//        行動力強化値とその継続ラウンド数を操作する。
//        強化値の範囲は-10～10、継続ラウンド数の範囲は0～999を越えない。
//        """
//        if cw.cwpy.ydata:
//            cw.cwpy.ydata.changed()
//        if self.is_unconscious():
//            value = 0
//            duration = 0
//        if value == 0:
//            duration = 0
//        if duration <= 0:
//            value = 0
//        self.enhance_act = value
//        self.enhance_act = cw.util.numwrap(self.enhance_act, -10, 10)
//        self.enhance_act_dur = duration
//        self.enhance_act_dur = cw.util.numwrap(self.enhance_act_dur, 0, 999)
//        path = "Property/Enhance/Action"
//        self.data.edit(path, str(self.enhance_act))
//        self.data.edit(path, str(self.enhance_act_dur), "duration")
//
//    def set_enhance_avo(self, value, duration):
//        """
//        回避力強化値とその継続ラウンド数を操作する。
//        強化値の範囲は-10～10、継続ラウンド数の範囲は0～999を越えない。
//        """
//        if cw.cwpy.ydata:
//            cw.cwpy.ydata.changed()
//        if self.is_unconscious():
//            value = 0
//            duration = 0
//        if value == 0:
//            duration = 0
//        if duration <= 0:
//            value = 0
//        self.enhance_avo = value
//        self.enhance_avo = cw.util.numwrap(self.enhance_avo, -10, 10)
//        self.enhance_avo_dur = duration
//        self.enhance_avo_dur = cw.util.numwrap(self.enhance_avo_dur, 0, 999)
//        path = "Property/Enhance/Avoid"
//        self.data.edit(path, str(self.enhance_avo))
//        self.data.edit(path, str(self.enhance_avo_dur), "duration")
//
//    def set_enhance_res(self, value, duration):
//        """
//        抵抗力強化値とその継続ラウンド数を操作する。
//        強化値の範囲は-10～10、継続ラウンド数の範囲は0～999を越えない。
//        """
//        if cw.cwpy.ydata:
//            cw.cwpy.ydata.changed()
//        if self.is_unconscious():
//            value = 0
//            duration = 0
//        if value == 0:
//            duration = 0
//        if duration <= 0:
//            value = 0
//        self.enhance_res = value
//        self.enhance_res = cw.util.numwrap(self.enhance_res, -10, 10)
//        self.enhance_res_dur = duration
//        self.enhance_res_dur = cw.util.numwrap(self.enhance_res_dur, 0, 999)
//        path = "Property/Enhance/Resist"
//        self.data.edit(path, str(self.enhance_res))
//        self.data.edit(path, str(self.enhance_res_dur), "duration")
//
//    def set_enhance_def(self, value, duration):
//        """
//        抵抗力強化値とその継続ラウンド数を操作する。
//        強化値の範囲は-10～10、継続ラウンド数の範囲は0～999を越えない。
//        """
//        if cw.cwpy.ydata:
//            cw.cwpy.ydata.changed()
//        if self.is_unconscious():
//            value = 0
//            duration = 0
//        if value == 0:
//            duration = 0
//        if duration <= 0:
//            value = 0
//        self.enhance_def = value
//        self.enhance_def = cw.util.numwrap(self.enhance_def, -10, 10)
//        self.enhance_def_dur = duration
//        self.enhance_def_dur = cw.util.numwrap(self.enhance_def_dur, 0, 999)
//        path = "Property/Enhance/Defense"
//        self.data.edit(path, str(self.enhance_def))
//        self.data.edit(path, str(self.enhance_def_dur), "duration")
//
//    def set_skillpower(self, value=999):
//        """
//        精神力(スキルの使用回数)を操作する。
//        recoveryがTrueだったら、最大値まで回復。
//        Falseだったら、0にする。
//        """
//        if cw.cwpy.ydata:
//            cw.cwpy.ydata.changed()
//        for header in self.get_pocketcards(cw.POCKET_SKILL):
//            header.set_uselimit(value)
//
//        if 0 < value:
//            if cw.cwpy.is_battlestatus():
//                self.deck.get_skillpower(self)
//        elif value < 0:
//            if cw.cwpy.is_battlestatus():
//                self.deck.lose_skillpower(self, -value)
//
    public void set_beast(UNK element=None, bool vanish=false, bool is_scenariocard=false)
    {
        // """召喚獣を召喚する。付帯召喚設定は強制的にクリアされる。
        // vanish: 召喚獣を消去するかどうか。
        // """
        UNK idx;
        bool eff;
        UNK etree;

        idx = cw.POCKET_BEAST;
        eff = false;
        if (vanish)
        {
            // for header in this.get_pocketcards(idx)[::-1]:
            //     if (not header.attachment)
            //     {
            //         this.throwaway_card(header, update_image=false)
            //         eff = true
            //       }
        }
        else if (this.can_addbeast())
        {
            if (this.is_unconscious())
            {
               return eff;
            }
            etree = cw.data.xml2etree(element=element, nocache=true);
            cw.content.get_card(etree, self, not is_scenariocard, update_image=false);
            eff = true;
            return eff;
        }
    }

    public void can_addbeast()
    {
        if (this.is_unconscious())
        {
           return false;
        }
        idx = cw.POCKET_BEAST;
        return len(this.get_pocketcards(idx)) < this.get_cardpocketspace()[idx];
    }

    public UNK decrease_physical(UNK stype, UNK time);
    {
        // """中毒麻痺の時間経過による軽減。""";
        for (int _t=0; _t<time; _t++)
        {
            uvalue = cw.util.div_vocation(this.get_vocation_val(("vit", "aggressive"))) + this.level + cw.cwpy.dice.roll(2);
            if (stype == "Poison") {
                tvalue = this.poison + cw.cwpy.dice.roll(2);
            } else {
                tvalue = this.paralyze + cw.cwpy.dice.roll(2);
            }

            flag = uvalue >= tvalue;
            dice = cw.cwpy.dice.roll(2);
            if (dice == 12) {
                flag = true;
            }
            if (dice == 2) {
                flag = false;
            }

            if (flag) {
                if (stype == "Poison") {
                    this.set_poison(-1);
                } else {
                    this.set_paralyze(-1);
                }
            }
        }
    }

    public UNK set_timeelapse(time=1, fromevent=false);
    {
        // """時間経過。"""
        if (cw.cwpy.ydata) {
            cw.cwpy.ydata.changed();
        }
        // 時限クーポン処理
        this.count_timedcoupon();
        oldalive = this.is_alive();
        flag = false; // 反転しながら画像を更新する場合はTrue
        updateimage = false; // 反転せずに画像を更新する場合はTrue

        // 中毒
        if (this.is_poison() && !this.is_unconscious()) {
            this.decrease_physical("Poison", time);

            if (!this.is_poison()) {
                flag = true;
                cw.cwpy.advlog.recover_poison(this);
            } else {
                cw.cwpy.play_sound("dump");
                value = 1 * this.poison;
                n = value / 5;
                n2 = value % 5 * 2;
                value = cw.cwpy.dice.roll(n, 10);

                if (n2) {
                    value += cw.cwpy.dice.roll(1, n2);
                }

                oldlife = this.life;
                value = this.set_life(-value);
                cw.cwpy.advlog.poison_damage(value, this.life, oldlife);

                if (this.status != "reversed" && this.status != "hidden") {
                    cw.animation.animate_sprite("lateralvibe", battlespeed=cw.cwpy.is_battlestatus());
                }
                this.update_image();
                cw.cwpy.draw(clip=this.rect);
            }
        }

        // 麻痺
        if (this.is_paralyze() && !this.is_petrified() && !this.is_unconscious()) {
            this.decrease_physical("Paralyze", time);
            if (!this.is_paralyze()) {
                cw.cwpy.advlog.recover_paralyze(this);
                flag = true;
            }
            if (this.is_analyzable()) {
                updateimage = true;
            }
        }

        // 束縛
        if (this.is_bind()) {
            value = this.bind - time;
            this.set_bind(value);
            if (!this.is_bind()) {
                cw.cwpy.advlog.recover_bind(this);
                flag = true;
            }
            if (this.is_analyzable()) {
                updateimage = true;
            }
        }

        // 沈黙
        if (this.is_silence()) {
            value = this.silence - time;
            this.set_silence(value);
            if (!this.is_silence()) {
                cw.cwpy.advlog.recover_silence(this);
                flag = true;
            }
            if (this.is_analyzable()) {
                updateimage = true;
            }
        }

        // 暴露
        if (this.is_faceup()) {
            value = this.faceup - time;
            this.set_faceup(value);
            if (!this.is_faceup()) {
                cw.cwpy.advlog.recover_faceup(this);
                flag = true;
            }
            if (this.is_analyzable()) {
                updateimage = true;
            }
        }

        // 魔法無効化
        if (this.is_antimagic()) {
            value = this.antimagic - time;
            this.set_antimagic(value);
            if (!this.is_antimagic()) {
                cw.cwpy.advlog.recover_antimagic(this);
                flag = true;
            }
            if (this.is_analyzable()) {
                updateimage = true;
            }
        }

        // 精神状態
        if (this.mentality_dur > 0) {
            value = this.mentality_dur - time;

            if (value > 0) {
                this.set_mentality(this.mentality, value);
            } else {
                cw.cwpy.advlog.recover_mentality(this.mentality);
                this.set_mentality("Normal", 0);
                flag = true;
            }

            if (this.is_analyzable()) {
                updateimage = true;
            }

        // 行動力
        if (this.enhance_act_dur > 0) {
            value = this.enhance_act_dur - time;

            if (value > 0) {
                this.set_enhance_act(this.enhance_act, value);
            } else {
                cw.cwpy.advlog.recover_enhance_act(this);
                this.set_enhance_act(0, 0);
                flag = true;
            }

            if (this.is_analyzable()) {
                updateimage = true;
            }

        // 回避力
        if (this.enhance_avo_dur > 0) {
            value = this.enhance_avo_dur - time;

            if (value > 0) {
                this.set_enhance_avo(this.enhance_avo, value);
            } else {
                cw.cwpy.advlog.recover_enhance_avo(this);
                this.set_enhance_avo(0, 0);
                flag = true;
            }

            if (this.is_analyzable()) {
                updateimage = true;
            }
        }

        // 抵抗力
        if (this.enhance_res_dur > 0) {
            value = this.enhance_res_dur - time;

            if (value > 0) {
                this.set_enhance_res(this.enhance_res, value);
            } else {
                cw.cwpy.advlog.recover_enhance_res(this);
                this.set_enhance_res(0, 0);
                flag = true;
            }

            if (this.is_analyzable()) {
                updateimage = true;
            }

        // 防御力
        if (this.enhance_def_dur > 0) {
            value = this.enhance_def_dur - time;

            if (value > 0) {
                this.set_enhance_def(this.enhance_def, value);
            } else {
                cw.cwpy.advlog.recover_enhance_def(this);
                this.set_enhance_def(0, 0);
                flag = true;
            }

            if (this.is_analyzable()) {
                updateimage = true;
            }

        // 中毒効果で死亡していたら、ステータスを元に戻す
        if (this.is_unconscious()) {
            this.set_unconsciousstatus();
        }

        // 画像更新;
        if (flag || updateimage) {
            if (this.status != "reversed" && this.status != "hidden") {
                if (flag) {
                    battlespeed = cw.cwpy.is_battlestatus();
                    cw.animation.animate_sprite("hide", battlespeed=battlespeed);
                    this.update_image();
                    cw.animation.animate_sprite("deal", battlespeed=battlespeed);
                } else {
                    this.update_image();
                }
            } else {
                this.update_image();
            }
            cw.cwpy.draw(clip=this.rect);
        }

        // エネミーまたはプレイヤー(Wsn.2)が中毒効果で死亡していたら、死亡イベント開始;
        if (isinstance((Player, Enemy)) && this.is_dead() && oldalive) { // TODO
            if (isinstance(Player)) { // TODO
                // プレイヤーカードのキーコード・死亡時イベント(Wsn.2);
                events = cw.cwpy.sdata.playerevents;
            } else {
                events = this.events;
            }

            if (events) {
                e_eventtarget = None;
                if (fromevent) {
                    for t in itertools.chain(cw.cwpy.get_pcards(), cw.cwpy.get_ecards(), cw.cwpy.get_fcards()) // TODO
                    {
                        if (isinstance(t, cw.character.Character)) { // TODO
                            if (t.has_coupon("＠イベント対象")) {
                                e_eventtarget = t;
                                t.remove_coupon("＠イベント対象");
                                break;
                            }
                        }
                    }
                }

                try
                {
                    if (cw.cwpy.sdata.is_wsnversion('2')) {
                        // イベント所持者を示すシステムクーポン(Wsn.2);
                        this.set_coupon("＠イベント対象", 0);
                    }
                    if (fromevent) {
                        event = events.check_keynum(1);
                        if (event) {
                            event.run_scenarioevent();
                        }
                    } else {
                        events.start(1, isinsideevent=false);
                    }
                }
                finally
                {
                    this.remove_coupon("＠イベント対象");

                    if (e_eventtarget) {
                        e_eventtarget.set_coupon("＠イベント対象", 0);
                    }
                }
            }
        }
    }

    public UNK set_hold_all(pocket, value)
    {
        this.hold_all[pocket] = value;
        if (pocket == cw.POCKET_SKILL) {
            type = "SkillCards";
        } else if (pocket == cw.POCKET_ITEM) {
            type = "ItemCards";
        } else if (pocket == cw.POCKET_BEAST) {
            type = "BeastCards";
        } else {
            assert false;
        }
        this.data.edit(type, str(value), "hold_all");
    }

}
class Player : Character {
    public void lost()
     {
        if (cw.cwpy.ydata)
        {
            cw.cwpy.ydata.changed();
        }
        this.remove_numbercoupon();
        this.remove_timedcoupons();
        this.data.edit("Property", "True", "lost");
        this.data.write_xml();
        if (cw.cwpy.is_playingscenario())
        {
            if (this.data.fpath.lower().startswith("yado"))
            {
                fpath = cw.util.relpath(this.data.fpath, cw.cwpy.ydata.yadodir);
            }
            else
            {
                fpath = cw.util.relpath(this.data.fpath, cw.cwpy.ydata.tempdir);
            }
            fpath = cw.util.join_paths(fpath);
            cw.cwpy.sdata.lostadventurers.add(fpath);
        }
        if (cw.cwpy.cardgrp.has(self))
        {
            cw.cwpy.cardgrp.remove(self);
            cw.cwpy.pcards.remove(self);
        }
    }

   public void set_name(UNK name)
   {
       Character.set_name(name);
       if (cw.cwpy.ydata)
       {
           for (header in cw.cwpy.ydata.partyrecord){
               header.rename_member(self.data.fpath, name);
           }
       }
       cw.cwpy.background.reload(false, nocheckvisible=true);
   }

}

public static F
{
    public static int calc_maxlife(int vit, int minval, int level)
    {
        // """能力値から体力の最大値を計算する。"""
        vit = max(1, vit);
        minval = max(1, minval);
        level = max(1, level);
        return int((float(vit) / 2.0 + 4) * (level + 1) + float(minval) / 2.0);
    }
}

class Enemy : Character
{
    public UNK is_dead()
    {
        // """
        // 敵は隠蔽状態であれば死亡と見做す。
        // """
        b = Character.is_dead();
        b = b || this.status == "hidden"; //b |= this.status == "hidden";
        return b;
    }
    public UNK is_inactive(UNK check_reversed=true)
    {
        // """
        // 敵は隠蔽状態であれば行動不能と見做す。
        // """
        b = Character.is_inactive(check_reversed=check_reversed);
        b = b || this.status == "hidden"; //b |= this.status == "hidden";
        return b;
    }
}

class Friend : Character{
   // pass;
}

class AlbumPage 
{
    private UNK data;
    private string name;
    private int level;
    // def __init__(self, data):
    public AlbumPage (UNK data)
    {
        this.data = data;
        this.name = this.data.gettext("Property/Name", "");
        this.level = cw.util.numwrap(this.data.getint("Property/Level"), 1, 65536);
    }

    public UNK get_specialcoupons()
    {
        // """
        // "＠"で始まる特殊クーポンの
        // 辞書(key=クーポン名, value=クーポン得点)を返す。
        // """
        Dictionary<string, UNK> d;
        d = {};

        for (e in this.data.getfind("Property/Coupons"))
        {
            coupon = e.text
            if (coupon && coupon.startswith(u"＠"))
            {
                d[coupon] = (int)e.get("value", "0")
            }
        }
        return d;
    }
}
