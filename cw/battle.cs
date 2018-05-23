public class BattleError : Exception
{
    public BattleError() { }
    public BattleError(string message) : base(message) { }
    public BattleError(string message, Exception inner) : base(message) { }
}

public class BattleAreaChangeError : Exception
{
    public BattleAreaChangeError() { }
    public BattleAreaChangeError(string message) : base(message) { }
    public BattleAreaChangeError(string message, Exception inner) : base(message) { }
}

public class BattleStartBattleError : Exception
{
    public BattleStartBattleError() { }
    public BattleStartBattleError(string message) : base(message) { }
    public BattleStartBattleError(string message, Exception inner) : base(message) { }
}

public class BattleWinError : Exception
{
    public BattleWinError() { }
    public BattleWinError(string message) : base(message) { }
    public BattleWinError(string message, Exception inner) : base(message) { }
}

public class BattleDefeatError : Exception
{
    public BattleDefeatError() { }
    public BattleDefeatError(string message) : base(message) { }
    public BattleDefeatError(string message, Exception inner) : base(message) { }
}

class BattleEngine;
{
    public List<UNK> priorityacts;
    public int round;
    public List<UNK> members;
    public bool _ready;
    public bool _running;
    public bool _ranaway;
    public bool numenemy;
    public bool in_roundevent;
    
    public BattleEngine() {
        // """
        // 戦闘関係のデータ・処理をまとめたクラス。
        // 初期化時に自動的にready()を実行する。
        // """
        // PlayerCard・FriendCardの戦闘用デッキを構築
        foreach(var pcard in cw.cwpy.get_pcards()) {
            pcard.deck.set(pcard, draw=false); // TODO
         }
 
        foreach(var fcard in cw.cwpy.get_fcards()) {
            fcard.deck.set(fcard, draw=false); // TODO
         }
 
        this.priorityacts = [];
 
        // ラウンド数
        this.round = 0;
        // 戦闘参加メンバ
        this.members = [];
        // 戦闘開始の準備完了フラグ
        this._ready = false;
        // 戦闘行動中フラグ
        this._running = false;
        // 前回のラウンドで逃走を試みた場合
        this._ranaway = false;
        // 表示中の敵の数
        this.numenemy = 0;
        if (cw.cwpy.is_autospread()) {
            this.numenemy = cw.cwpy.get_mcards("flagtrue").Count; // TODO
        }
        // ラウンドイベント中か
        this.in_roundevent = false;
 
        cw.cwpy.battle = this;
 
        try {
            cw.cwpy.advlog.start_battle();
 
            // バトル開始イベント(1.50)
            // このイベントの終了時点では勝利・敗北は発生しない
            cw.cwpy.sdata.start_event(keynum=5); // TODO
 
            // 行動準備
            this.ready();
 
        } catch(BattleStartBattleError) {
            this.end(false, startnextbattle=true); // TODO
        } catch(BattleAreaChangeError) {
            this.end(false);
        } catch(BattleWinError) {
            this.win(runevent=false); // TODO
        } catch(BattleDefeatError) {
            this.defeat(runevent=false); // TODO
        }
    }
 
    public bool is_running() {
        return this._running;
    }
 
    public bool is_ready() {
        return this._ready;
    }
 
    public bool is_battlestarting() {
        return !(this.is_running() || this.is_ready());
    }
 
    public void start() {
        try {
            this.run();
        } catch(BattleStartBattleError) {
            this.end(false, startnextbattle=true); // TODO
        } catch(BattleAreaChangeError) {
            this.end(false);
        } catch(BattleWinError) {
            this.win();
        } catch(BattleDefeatError) {
            this.defeat();
        }
    }
 
    public void process_exception(ex) {
         // """イベントの強制実行等で発生したバトル例外を処理する。"""
        if (isinstance(ex, BattleStartBattleError)) {
            this.end(false, startnextbattle=true); // TODO
        } else if (isinstance(ex, BattleAreaChangeError)) {
            this.end(false);
        } else if (isinstance(ex, BattleWinError)) {
            this.win();
        } else if (isinstance(ex, BattleDefeatError)) {
            this.defeat();
        } else {
            Debug.Assert(false);
        }
    }
 
    public void run() {
         // """戦闘行動を開始する。1ラウンド分の処理。"""
        cw.cwpy.clear_selection();
        cw.cwpy.clear_fcardsprites();
 
        if (!cw.cwpy.is_playingscenario() || cw.cwpy.sdata.in_f9) {
            this.end(f9=true); //TODO
            return;
        }
 
        this._running = true;
        this._ready = false;
 
        // デバッグ操作などで状態が変わっている可能性があるため
        // 勝利・敗北チェック
        if (this.check_defeat()) {
            throw new BattleDefeatError();
        } else if (this.check_win()) {
            throw new BattleWinError();
        }
 
        cw.cwpy.advlog.start_round(this.round);
 
        // ラウンドイベントスタート
        this.in_roundevent = true;
        try {
            cw.cwpy.sdata.start_event(keynum=-this.round); // TODO
        } finally {
            this.in_roundevent = false;
        }
 
        if (!cw.cwpy.is_playingscenario() || cw.cwpy.sdata.in_f9) {
            this.end(f9=true); // TODO
            return;
        }
 
        // イベント結果の勝利・敗北チェック
        if (this.check_defeat()) {
            throw new BattleDefeatError();
        } else if (this.check_win()) {
            throw new BattleWinError();
        }
 
        clip = cw.cwpy.update_statusimgs(is_runningevent=true); // TODO
        if (clip) {
            cw.cwpy.draw(clip=clip); // TODO
        }
 
        // 戦闘行動ループ
        foreach(var member in this.members) {
            member.actionend = false;
        }
        foreach(var member in this.members) {
            if (member.actionend) {
                continue;
            }
            cw.cwpy.input();
            cw.cwpy.eventhandler.run();
            member.action();
            if (!cw.cwpy.is_playingscenario() || cw.cwpy.sdata.in_f9) {
                this.end(f9=true); // TODO
                return;
            }
            if (!this._running) {
                return;
            }
 
            // 勝利チェック
            if (this.check_win()) {
                throw new BattleWinError();
            }
		}
 
        // 行動内容のクリア
        foreach(var member in this.members) {
            member.deck.clear_used();
            member.clear_action();
        }
 
        if (!cw.cwpy.is_playingscenario() || cw.cwpy.sdata.in_f9) {
            this.end(f9=true); // TODO
            return;
        }
 
        cw.cwpy.input();
        cw.cwpy.eventhandler.run();
 
        // 時間経過
        cw.cwpy.elapse_time();
 
        // 勝利・敗北チェック
        if (this.check_defeat()) {
            throw new BattleDefeatError();
        } else if (this.check_win()) {
            throw new BattleWinError();
        }
 
        ran = this._ranaway;
        this._running = false;
        this._ranaway = false;
 
        // 2ラウンド目移行は自動で行動開始可能
        // ただし逃走を試みた時は自動で行動開始はしたくないはずなので開始しない
        if (cw.cwpy.setting.show_roundautostartbutton && cw.cwpy.sdata.autostart_round && !ran) {
            this.ready(redraw=false); // TODO
            cw.cwpy.exec_func(this.start);
        } else {
            // 次ターン準備
            this.ready();
        }
    }
 
    public void end(areachange=true, f9=false, startnextbattle=false) {
         // """勝利・敗北以外の戦闘終了処理。
         // 戦闘エリアを解除する。
         // """
        // 対象選択中であれば中止
        cw.cwpy.clean_specials();
 
        // 行動内容のクリア
        foreach(var member in cw.cwpy.get_pcards()) {
            member.clear_action();
        }
 
        is_battlestarting = this.is_battlestarting();
        this._running = false;
        this._ready = true;
 
        if (f9) {
            cw.cwpy.sdata.pre_battleareadata = None;
        } else {
            if (!startnextbattle) {
                cw.cwpy.advlog.end_battle();
            }
            cw.cwpy.clear_battlearea(areachange=areachange, startnextbattle=startnextbattle, is_battlestarting=is_battlestarting); // TODO
        }
    }
            
 
    public void ready(redraw=true) {
         // """戦闘行動の準備を行う。
         // 1ラウンド終了するたびに自動的に呼ばれる。
         // """
        this.round += 1;
        this.round = cw.util.numwrap(this.round, 1, 999999);
        // 戦闘参加メンバセット・行動順にソート・手札自動選択
        this.priorityacts = [];
        this.set_members();
        // 山札からカードをドロー
        foreach(var member in this.members) {
            member.deck.draw(member);
        }
        this.set_actionorder();
        this.set_action();
 
        if (cw.cwpy.is_autospread()) {
            ecards = cw.cwpy.get_mcards("flagtrue");
            if (this.numenemy <> ecards.Count) {
                this.numenemy = ecards.Count;
                cw.cwpy.set_autospread(ecards, 6, false, anime=true);
            }
		}
        cw.cwpy.show_party();
        cw.cwpy.disposition_pcards();
        if (cw.cwpy.is_debugmode() && cw.cwpy.setting.show_fcardsinbattle) {
            cw.cwpy.add_fcardsprites(status="normal", alpha=192);
        }
        if (redraw) {
            this._ready = true;
            cw.cwpy.statusbar.change();
            cw.cwpy.clear_selection();
            cw.cwpy.draw();
        }
    }
 
    public void update_debug() {
        // 敵の状態の暴露・非暴露切り替え
        foreach(var sprite in cw.cwpy.get_mcards()) {
            sprite.update_scale();
        }
 
        // 同行NPCの表示切り替え
        if (this.is_ready()) {
            this.update_showfcards();
        }
    }
 
    public void update_showfcards() {
        cw.cwpy.clear_fcardsprites();
        if (cw.cwpy.is_debugmode() && cw.cwpy.setting.show_fcardsinbattle && this.is_ready()) {
            cw.cwpy.add_fcardsprites(status="normal", alpha=192);
        }
    }
 
    public void runaway() {
         // """逃走処理。逃走イベントが存在する場合は、
         // 逃走イベント優先。
         // """
        cw.cwpy.clear_fcardsprites();
        this.clear_playersaction();
        event = cw.cwpy.sdata.events.check_keynum(2);
        this._ranaway = true;
 
        cw.cwpy.advlog.start_runaway();
 
        if (event) {
            // 逃走イベント開始
            try {
                cw.cwpy.clear_selection();
                cw.cwpy.clear_fcardsprites();
                this._ready = false;
                this._running = true;
                cw.cwpy.sdata.start_event(keynum=2);
            } catch(BattleStartBattleError) {
                this.end(false, startnextbattle=true); // TODO
            } catch(BattleAreaChangeError) {
                this.end(false);
            } catch(BattleDefeatError) {
                this.defeat();
            } catch(BattleError) {
                this.start();
            } else {
                this.start();
            }
 
        } else {
            // 判定値を算出
            ecards = cw.cwpy.get_ecards("active");
            level = sum([ecard.level for ecard in ecards]); // TODO
            level = level / ecards.Count if ecards else 0; // TODO
            vocation = ("agl", "trickish");
            enemybonus = ecards.Count + 3;
            // パーティ全員で敏捷・狡猾の行為判定
            // 半分以上が判定成功したら、逃走成功
            pcards = cw.cwpy.get_pcards("active");
            success = [pcard.decide_outcome(level, vocation, enemybonus) for pcard in pcards].count(true); //TODO
 
            // 逃走成功・失敗時の処理
            if (pcards && success > pcards.Count / 2) {
                cw.cwpy.advlog.runaway(true);
                // 行動内容のクリア
                foreach(var member in this.members) {
                    member.clear_action();
                }
                cw.cwpy.play_sound("run");
                this.end();
                cw.cwpy.statusbar.change(true);
            } else {
                cw.cwpy.advlog.runaway(false);
                cw.cwpy.play_sound("error");
                this.start();
            }
        }
    }
 
    public void win(runevent=true) {
         // """勝利処理。勝利イベント終了後も戦闘が続行していたら、
         // 強制的に戦闘エリアから離脱する。
         // """
        // 行動内容のクリア
        foreach(var member in cw.cwpy.get_pcards()) {
            member.clear_action();
        }
        assert cw.cwpy.is_battlestatus();
 
        is_battlestarting = this.is_battlestarting();
        cw.cwpy.hide_cards(true);
        cw.cwpy.cardgrp.remove(cw.cwpy.mcards);
        cw.cwpy.mcards = [];
        cw.cwpy.file_updates.clear();
 
        if (runevent) {
            eventkeynum = 1;
        } else {
            eventkeynum = 0;
        }
 
        cw.cwpy.advlog.end_battle();
 
        // 勝利イベント実行時は元のエリアに戻る
        cw.cwpy.clear_battlearea(true, eventkeynum=eventkeynum, is_battlestarting=is_battlestarting); // TODO
    }
 
    public void defeat(runevent=true) {
         // """敗北処理。敗北イベント後、
         // パーティが全滅状態だったら、ゲームオーバ画面に遷移。
         // """
        // 行動内容のクリア
        foreach(var member in this.members) {
            member.clear_action();
        }
 
        is_battlestarting = this.is_battlestarting();
        this._running = false;
        if (runevent && !cw.cwpy.is_forcegameover()) {
            event = cw.cwpy.sdata.events.check_keynum(3);
        } else {
            event = None;
        }
 
        cw.cwpy.advlog.end_battle();
 
        if (event) {
            cw.cwpy.hide_cards(true);
            cw.cwpy.cardgrp.remove(cw.cwpy.mcards);
            cw.cwpy.mcards = [];
            cw.cwpy.file_updates.clear();
            cw.cwpy._gameover = false;
 
            // 戦闘前のエリアに戻り、敗北イベント開始
            cw.cwpy.clear_battlearea(true, eventkeynum=3, is_battlestarting=is_battlestarting); // TODO
 
        } else {
            cw.cwpy.set_gameover();
        }
    }
 
    public bool check_win() {
        bool flag = true;
 
        foreach(var ecard in cw.cwpy.get_ecards()) {
            if (ecard.is_alive()) {
                flag = false;
            }
        }
 
        return flag;
    }
 
    public bool check_defeat() {
        bool flag = true;
 
        foreach(var pcard in cw.cwpy.get_pcards()) {
            if (pcard.is_alive()) {
                flag = false;
            }
        }
 
        return flag;
    }
 
    public void set_members() {
         // """戦闘参加メンバを設定する。
         // 行動可能でないものは除外。
         // """
        members = cw.cwpy.get_pcards("unreversed");
        members.extend(cw.cwpy.get_ecards("unreversed"));
        members.extend(cw.cwpy.get_fcards());
        this.members = members;
    }
 
    public void set_actionorder() {
         // """行動順を決める値を算出し、
         // その値をもとに並び替えした戦闘参加メンバを設定する。
         // """
        if (!this.members) {
            return;
        }
 
        member = this.members[0];
        members = [(-member.decide_actionorder(), 0, member)]; // TODO
        for( i, member in enumerate(this.members[1:], 1)) { // TODO
            o = (-member.decide_actionorder(), -i, member);
            bisect.insort(members, o);
        }
 
        Debug.Assert(members.Count == this.members.Count);
        this.members = map(lambda o: o[2], members); // TODO
    }
 
    public void set_action() {
         // """戦闘参加メンバ全員、行動自動選択。"""
        foreach(var member in this.members) {
            member.decide_action();
        }
	}
 
    public void clear_playersaction() {
         // """PlayerCard, FriendCardの行動をクリアする。"""
        foreach(var pcard in cw.cwpy.get_pcards()) {
            pcard.clear_action();
        }
 
        foreach(var fcard in cw.cwpy.get_fcards()) {
            fcard.clear_action();
        }
    }
}
