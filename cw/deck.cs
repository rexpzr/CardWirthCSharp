class Deck
{
    private List<UNK> hand;
    private List<UNK> talon;
    private List<UNK> nextcards;
    private bool _throwaway;
    private UNK? _used;

    public Deck(UNK ccard)
    {
        // 手札
        this.hand = new List<UNK>();
        // 山札
        this.talon = new List<UNK>();
        // 定められた次のドローカード
        this.nextcards = new List<UNK>();
        // 手札が破棄されたか
        this._throwaway = false;
        this._used = null;
    }

    public List<UNK> get_hand(UNK ccard)
    {
        // """ccardの手札に存在すると仮定されるカードのlistを返す。"""
        List<UNK> seq;
        if (this.is_throwed() ||  !ccard.is_active())
        {
            seq = new List<UNK>();
        }else{
            seq = this.hand[:]; //TODO
        }
        if (this._used)
        {
            seq.append(this._used);
        }
        return seq;
    }

    public UNK get_used()
    {
        return this._used;
    }

    public void clear_used()
    {
        // 使用後の残存カードをクリアする。
        this._used = null;
    }

    public List<UNK> get_actioncards(UNK ccard)
    {
        List<UNK> seq = new List<UNK>();
        foreach (var resid, header in cw.cwpy.rsrc.actioncards.iteritems())
        {
            if (resid == 7 && !ccard.escape)
            {
                continue; // 逃走しない
            }
            if (resid > 0)
            {
                foreach (var _cnt in xrange(header.uselimit))
                {
                    seq.append(header);
                }
            }
        }

        return seq;
    }

    public List<UNK> get_skillcards(UNK ccard, UNK handcounts={}.copy())
    {
        List<UNK> seq = new List<UNK>();
        foreach (var header in ccard.get_pocketcards(cw.POCKET_SKILL))
        {
            uselimit, _maxn = header.get_uselimit(); // TODO
            foreach (var _cnt in xrange(uselimit - handcounts.get(header, 0)))
            {
                seq.append(header);
            }
        }
        return seq;
    }

    public void set_nextcard(self, resid=0)
    {
        // """山札の一番上に指定したIDのアクションカードを置く。
        // IDを指定しなかった場合(0の場合)は、スキルカードを置く。
        // """
        this.nextcards.insert(0, resid);
    }

    public UNK _set_nextcard(self, resid, brave=false)
    {
        // アクションカード
        if (resid && resid in cw.cwpy.rsrc.actioncards)
        {
            header = cw.cwpy.rsrc.actioncards[resid];
        // スキルカード
        }else{
            bool _for_else = true;
            foreach (var header in this.talon) 
            {
                if (header.type == "SkillCard")
                {
                    _for_else = false;
                    break;
                }

            }
            if (_for_else) {
                if (brave)
                {
                    // スキルカードが尽き、かつ勇敢ならば、配布されるカードとして攻撃系を指定
                    header = cw.cwpy.rsrc.actioncards[cw.cwpy.dice.roll(1, 3)];
                }else{
                    return;
                }
            }
        }

        // 山札の一番上へカードを置く
        if (!resid < 0 && header in this.talon)
        {
            this.talon.remove(header);
        }
        this.talon.append(header);
    }

    public void shuffle(self)
    {
        this.talon = cw.cwpy.dice.shuffle(this.talon);
    }

    public void shuffle_bottom(self)
    {
        try{
            talon = cw.cwpy.dice.shuffle(this.talon[:-20])
            talon.extend(this.talon[-20:])
            this.talon = talon
        } catch(Exception e){
            // pass;
        }
    }

    public int get_handmaxnum(self, ccard)
    {
        n = (ccard.level + 1); // 2 + 4
        n = cw.util.numwrap(n, 5, 12);
        return n;
    }

    public void set(self, ccard, draw=true)
    {
        this.clear(ccard);
        this.talon.extend(this.get_actioncards(ccard));
        this.talon.extend(this.get_skillcards(ccard));
        this.shuffle();
        this.set_hand(ccard);
        if (draw)
        {
            this.draw(ccard);
        }
    }

    public void set_hand(UNK ccard)
    {
        var hand = new List<UNK>();
        foreach(var h in this.hand) {
            if (h.type == "SkillCard" || h.type == "ActionCard" && h.id > 0) {
                hand.Add(h);
            }
        }

        // 手札構築
        this.hand = new List<UNK>();
        // カード交換カードを手札に加える
        header = cw.cwpy.rsrc.actioncards[0].copy();
        header.set_owner(ccard);
        this.hand.append(header);
        // アイテムカードを手札に加える
        this.hand.extend(ccard.get_pocketcards(cw.POCKET_ITEM));
        // アクションカード、技能カードを手札に加える
        maxn = this.get_handmaxnum(ccard);
        index = maxn - len(this.hand);
        this.hand.extend(hand[:index]);
        flag = false;

        foreach (var header in hand[index:])
        {
            if (header.type == "SkillCard")
            {
                header = header.ref_original();
                this.talon.insert(0, header);
                flag = true;
            }else if (header.type == "ActionCard" && header.id > 0){
                header = cw.cwpy.rsrc.actioncards[header.id];
                this.talon.insert(0, header);
                flag = true;
            }
        }
        if (flag)
        {
            this.shuffle_bottom();
        }
    }

    public void add(UNK ccard, UNK header)
    {
        if (header.type == "ItemCard")
        {
            this.set_hand(ccard);
        }else if (header.type == "SkillCard"){
            uselimit, _maxn = header.get_uselimit();

            foreach (var _cnt in xrange(uselimit))
            {
                this.talon.append(header);
            }
            this.shuffle();
        }
    }

    public void remove(UNK ccard, UNK header)
    {
        if (cw.cwpy.battle)
        {
            this.hand = [h for h in this.hand if !h.ref_original == header.ref_original];
            this.talon = [h for h in this.talon if !h.ref_original == header.ref_original];
        }
    }

    public UNK get_skillpower(UNK ccard)
    {
        // 一旦山札から全てのスキルを取り除く
        List<UNK> talon = new List<UNK>;
        foreach (var header in this.talon)
        {
            if (header.type != "SkillCard")
            {
                talon.append(header);
            }
        }
        this.talon = talon;

        // 現在手札にある分と配付予約にある分をカウントする
        Dictionary<UNK, UNK> handcounts = new Dictionary<UNK, UNK>();
        foreach (var header in this.hand)
        {
            header = header.ref_original();
            count = handcounts.get(header, 0);
            count += 1;
            handcounts[header] = count;
        }

        // 山札に改めて追加
        this.talon.extend(this.get_skillcards(ccard, handcounts));
        this.shuffle();

        this._update_skillpower(ccard);
    }


    private void lose_skillpower__remove_skill(Dictionary<UNK, int> skilltable, UNK losevalue, UNK header, UNK seq) {
        if (header.type == "SkillCard") {
            UNK orig = header.ref_original();
            int removecount = skilltable.get(orig, 0);
            if (removecount < losevalue) {
              skilltable[orig] = removecount + 1;
              return;
            }
        }
        seq.append(header);
    }

    public void lose_skillpower(UNK ccard, UNK losevalue)
    {
        // 現在handにある分は除去しなくてよい
        var talon = new List<UNK>();
        var skilltable = new Dictionary<UNK, int>();

        foreach (var header in this.talon) {
            lose_skillpower__remove_skill(skilltable, losevalue, header, talon);
        }
        this.talon = talon;
        this.shuffle();
        if (this._throwaway) {
            // 手札喪失が予約されている場合に限りhandからも除去
            var hand = new List<UNK>();
            foreach (var header in this.hand) {
                lose_skillpower__remove_skill(skilltable, losevalue, header, hand);
            }
            this.hand = hand;
        }

        this._update_skillpower(ccard);
    }


    public void _update_skillpower(UNK ccard)
    {
        // 手札と山札にある数によってスキルカードの使用回数を更新する
        Dictionary<UNK, UNK> handcounts = new Dictionary<UNK, UNK>();
        foreach (var header in itertools.chain(this.hand, this.talon))
        {
            if (header.type == "SkillCard")
            {
                header = header.ref_original();
                count = handcounts.get(header, 0);
                count += 1;
                handcounts[header] = count;
            }
        }

        foreach (var header in ccard.get_pocketcards(cw.POCKET_SKILL))
        {
            count = handcounts.get(header, 0);
            header.set_uselimit(count - header.uselimit);
        }

        foreach (var header in itertools.chain(this.hand, this.talon))
        {
            if (header.type == "SkillCard")
            {
                count = handcounts.get(header.ref_original(), 0);
                header.uselimit = count;
            }
        }
    }

    public void update_skillcardimage(UNK header)
    {
        foreach (var header2 in this.hand)
        {
            if (header2.ref_original() is header)
            {
                header2.uselimit = header.uselimit;
            }
        }
    }

    public void clear(UNK ccard)
    {
        this.talon = new List<UNK>();
        this.hand = new List<UNK>;
        this.nextcards = new List<UNK>;
        this._throwaway = false;
        this._used = null;
        ccard.clear_action();
    }

    public void throwaway()
    {
        // """手札消去効果を適用する。"""
        this._throwaway = true;
    }

    public UNK is_throwed()
    {
        // """手札が消去されているか。"""
        return this._throwaway
    }

    public void _remove(UNK header)
    {
        this.hand.remove(header);
        if (header.type == "SkillCard")
        {
            header = header.ref_original();
            this.talon.append(header);
        }else if (header.type == "ActionCard" && header.id > 0){
            header = cw.cwpy.rsrc.actioncards[header.id];
            this.talon.append(header);
        }
    }

    public void draw(UNK ccard)
    {
        this._used = null;
        UNK maxn = this.get_handmaxnum(ccard);
        if (this._throwaway)
        {
            // 現在の手札を山札に戻す
            foreach (var header in this.hand[1::])
            {
                this._remove(header);
            }
            this.shuffle();

            this.hand = new List<UNK>;
            // カード交換は常に残す
            header = cw.cwpy.rsrc.actioncards[0].copy();
            header.set_owner(ccard);
            this.hand.append(header);
            // アイテムカードを手札に加える
            this.hand.extend(ccard.get_pocketcards(cw.POCKET_ITEM));
            this._throwaway = false;
        }

        while (this.hand.Count < maxn)
        {
            if (this.nextcards)
            {
                this._set_nextcard(this.nextcards.pop(), ccard.is_brave());
            }else{
                this.check_mind(ccard);
            }

            header = this.talon.pop();
            header_copy = header.copy();

            if (header.type == "ActionCard")
            {
                header_copy.set_owner(ccard);
            }

            this.hand.append(header_copy);
        }

        while (maxn < this.hand.Count)
        {
            this._remove(this.hand[-1]);
        }
    }

    public void check_mind(UNK ccard)
    {
        // """
        // 特殊な精神状態の場合、次にドローするカードを変更。
        // """
        if (ccard.is_panic())
        {
            if (ccard.escape)
            {
                n = cw.cwpy.dice.roll(1, 3) + 4;
            }else{
                n = cw.cwpy.dice.roll(1, 2) + 4;
            }
            this._set_nextcard(n);
        }else if (ccard.is_brave()){
            n = cw.cwpy.dice.roll(1, 4) - 1;
            this._set_nextcard(n, brave=true);
        }else if (ccard.is_overheat()){
            this._set_nextcard(2);
        }else if (ccard.is_confuse()){
            // 混乱時、混乱カードは2/3の確率で配布とする。
            if (cw.cwpy.dice.roll(1, 3)>1)
            {
                this._set_nextcard(-1);
            }
        }
    }

    public void use(UNK header)
    {
        // """headerを使用する。
        // アイテムカードまたはカード交換は手札に残る。
        // スキルカードは1枚消失する。
        // アクションカードは山札に戻る。
        // """
        this._used = header;
        if (header in this.hand && !header.type == "ItemCard" && !(header.type == "ActionCard" && header.id == 0))
        {
            this.hand.remove(header);
            if (header.type == "ActionCard" && 0 <= header.id)
            {
                this.talon.insert(0, header);
            }
        }
    }
}
