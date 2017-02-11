Explanation of test scripts:

Good Scripts:
1. Default provided script
2. Wrong coins loaded should affect change (i.e. if change=5c, but loaded with 25c, change should be 25c)
3. Loaded coins cannot match change value should short change
4. Machine can be reconfigured to accept different pops
5. An incorrect pop can be loaded
6. An incorrect coin can be loaded
7. Incorrect coin insertions ends up in dispenser
8. Check wrong pop load on teardown
9. Check wrong coin load on teardown
10. Check bank, and pop and coins slots are empty after teardown
11. In limbo value after no valid selection made (i.e. 40c inserted) - this value should not be included in teardown
12. If not enough pop, nothing should happen (still has credit, able to buy another pop)
13. If not enough money, nothing should happen (still has credit, able to buy another pop)
14. If no change left, then inserted money used (as normal) and any discrepancy is left as credit
15. Credit remaining due to invalid denomations (i.e. 33c with only 5c coins)
16. Credit remaining (no change given - i.e. full credit) due to invalid denomations (i.e. 50c with only 100c coins)
17. Multiple machine instances
18. If credit in machine on teardown, this is not included on teardown and can be used again when machine re-stocked

Bad Scripts:
1. Default provided script
2. Default provided script
3. Invalid pop name (with number)
4. Invalid coin value (0)
5. Invalid pop name (with symbol)
6. Duplicate coin value
7. More pops than slots
8. Invalid coin value (negative)
9. Invalid pop price (0)
10. Invalid pop price (negative)
11. Empty machine parameters
12. Less pops than slots
13. If all coin slots and storage bin are full