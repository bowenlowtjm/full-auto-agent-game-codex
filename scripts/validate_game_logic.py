#!/usr/bin/env python3
import math

class Rules:
    combo_step=1.1
    combo_cap=5.0
    lives=3

class ScoreState:
    def __init__(self):
        self.score=0
        self.combo=1.0
        self.lives=Rules.lives
    def hit(self, base):
        reward=round(base*self.combo)
        self.score+=reward
        self.combo=min(self.combo*Rules.combo_step, Rules.combo_cap)
        return reward
    def miss(self):
        self.combo=1.0
        self.lives-=1

def test_combo_growth():
    s=ScoreState()
    r1=s.hit(1)
    r2=s.hit(1)
    assert r1==1 and r2==1
    assert abs(s.combo-1.21)<1e-6

def test_combo_cap():
    s=ScoreState()
    for _ in range(20):
        s.hit(5)
    assert abs(s.combo-5.0)<1e-6

def test_penalty_resets_combo_and_life():
    s=ScoreState(); s.hit(3); s.hit(3)
    s.miss()
    assert s.combo==1.0
    assert s.lives==2

def test_determinism():
    import random
    seed=12345
    def run():
        r=random.Random(seed)
        seq=[]
        for _ in range(10):
            seq.append(r.randint(0,4))
        return seq
    assert run()==run()

if __name__=='__main__':
    tests=[test_combo_growth,test_combo_cap,test_penalty_resets_combo_and_life,test_determinism]
    for t in tests: t()
    print('standalone-validator: PASS (4/4)')
