using UnityEngine;
using NPBehave;
using System.Collections.Generic;

namespace Complete
{
    /*
    Example behaviour trees for the Tank AI.  This is partial definition:
    the core AI code is defined in TankAI.cs.

    Use this file to specifiy your new behaviour tree.
     */
    public partial class TankAI : MonoBehaviour
    {
        private Root CreateBehaviourTree() {

            switch (m_Behaviour) {

                case 0:
					//fun
                    //return Class0(1f, 1f,20,15,0.4f);
					return ClassMagna(1f, 1f, 0.15f, 30, 0.7f, 15, true, true, false, false, true, true, false, true);
				case 1:
					//deadly
                    //return Class1(1f, 1f, 40, 15);
					return ClassMagna(1f, 1f, 0f, 40, 0.6f, 12, true, true, false, true, true, true, true, true);
				case 2:
					//frightened 
					//return Class2(1f, 1f, 25, 25);
					return ClassMagna(1f, 1f, 0f, 25, 1.3f, 25, true, true, true, true, false, true, true, false);
				case 3:
					//64 combinations of random
					bool[] F = new bool[8];

					for (int i = 0; i < F.Length ; i++)
					{
						int j = UnityEngine.Random.Range(0, 2);
						F[i] = j == 1 ? true : false;
					}
					//*** literally craft your own AI
					return ClassMagna(1f, 1f, UnityEngine.Random.Range(0, 0.15f), UnityEngine.Random.Range(20, 40), UnityEngine.Random.Range(0.7f, 1.3f), 15, F[0], F[1], F[2], F[3], F[4], F[5], F[6], F[7]);

				case 4:

					//*** debugging
					//*** literally craft your own AI
					return ClassMagna(1f, 1f, 0f, 40, 0.7f, 30, false, true, false, true, false, false, false, false);

				case 5:
					//*** debugging
					//*** literally craft your own AI
					return ClassMagna(1f, 1f, 0f, 100, 0.7f, 15, false, false, false, false, false, false, false, true);

				case 6:
					//*** debugging
					//*** literally craft your own AI
					return ClassMagna(1f, 1f, 0f, 40, 0.7f, 20, true, false, false, true, true, true, true, true);

				case 7:
					//*** debugging
					//*** literally craft your own AI
					return ClassMagna(1f, 1f, 0f, 40, 0.7f, 20, true, false, false, false, false, false, false, false);

				default:
                    return new Root (new Action(()=> Turn(0.1f)));
            }
        }

        /* Actions */

        private Node StopTurning() {
            return new Action(() => Turn(0));
        }


		//return the nearest shell to AI
		private float seeShell()
		{
			ShellExplosion[] shells = GameObject.FindObjectsOfType(typeof(ShellExplosion)) as ShellExplosion[];
			float nearestShell = float.MaxValue;

			foreach (ShellExplosion shell in shells)
			{
				Vector3 targetPos = shell.transform.position;
				Vector3 localPos = this.transform.InverseTransformPoint(targetPos);
				Vector3 heading = localPos.normalized;

				//print(shell.transform.forward.x + "  " + this.transform.forward.x);

				float distance = localPos.magnitude;
				//ignore it's own shells to deem 'nearest' shell
				if (distance < nearestShell && Mathf.Abs(shell.transform.forward.x - this.transform.forward.x) > 0.1f)
				{

					nearestShell = distance;
				}
				else
				{
					//print(shell.transform.forward.x - this.transform.forward.x);
				}
			}
			return nearestShell;
		}

		//no difference to seeShell
		private ShellExplosion getNearestShell()
		{
			ShellExplosion[] shells = GameObject.FindObjectsOfType(typeof(ShellExplosion)) as ShellExplosion[];
			ShellExplosion nearestShell = shells[shells.Length-1];

			foreach (ShellExplosion shell in shells)
			{
				Vector3 targetPos = shell.transform.position;
				Vector3 localPos = this.transform.InverseTransformPoint(targetPos);
				Vector3 heading = localPos.normalized;


				Vector3 nearestPos = nearestShell.transform.position;
				Vector3 nearestLocalPos = this.transform.InverseTransformPoint(nearestPos);
				Vector3 nearestHeading = nearestLocalPos.normalized;

				//ignore it's own shells to deem 'nearest' shell
				if (localPos.magnitude < nearestLocalPos.magnitude && Mathf.Abs(shell.transform.forward.x - this.transform.forward.x) > 0.1f)
				{
					nearestShell = shell;
				}
			}
			return nearestShell;
		}

		//returns best turn outcome for dodging the closest bullet
		private float getDodgeAngle(ShellExplosion shell, bool defensiveDodge)
		{
			Vector3 shellDirection = shell.transform.forward;
			shellDirection.y = 0;

			//i remember i had to do dot products just to figure out it was orthogonal :*
			Vector3 outcome = Quaternion.AngleAxis(90, Vector3.up) * shellDirection;

			Vector3 localDirection = this.transform.forward;

			Vector3 angle = this.transform.InverseTransformDirection(outcome);

			int turnType = 1;
			if (defensiveDodge)
			{
				turnType = -1;
			}


			//print(angle + "            " + this.transform.InverseTransformPoint(shell.transform.position).magnitude);

				if (angle.x >= 0.1f )
				{
					print("right");
					return 1 * turnType;
				}
				else if(angle.x <= -0.1f)
				{
					print("left");
					return -1 * turnType;
				}

			print("forward");
			return 0;

		}


		private Node getDodgeTurn(bool defensiveDodge)
		{
			return new Action(() => Turn(getDodgeAngle(getNearestShell(), defensiveDodge)));
		}





		//raycast from AI to enemy
		private bool InSight()
		{
			Vector3 targetPos = new Vector3(TargetTransform().position.x, TargetTransform().position.y + 0.5f, TargetTransform().position.z);
			Vector3 thisPos = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
			RaycastHit hit;
			if (Physics.SphereCast(transform.position,0.25f, targetPos - transform.position, out hit)) {
				if (hit.collider.gameObject.name.Contains("Tank")) {
					return true;
				}
			}
			return false;
		}

		//get nearest gameObject's distance via raycasting
		private float ObjectDistance(Vector3 direction)
		{
			Vector3 thisPos = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

			//Debug.DrawRay(thisPos, direction * 50, Color.green);
			RaycastHit hit;
			if (Physics.SphereCast(transform.position,0.25f, direction, out hit))
			{
				return hit.distance;
			}
			return 0;
		}



		private Node getNextTurn()
		{
			return new Action(() => Turn(CalNextTurn()));
		}

		//determines next Turn through raycasting (Bottom-up approach), cos i dont know how to use navmesh 
		private float CalNextTurn()
		{
			float right = ObjectDistance(this.transform.right);
			float left = ObjectDistance(-1 * this.transform.right);
			float forward = ObjectDistance(this.transform.forward);

			if ((right > forward || left > forward) && forward > 6)
			{
				if (right > left)
				{
					return 1;
				}
				else
				{
					return -1;
				}
			}
			if (forward < 6)
			{
				if (right > left)
				{
					return 1;
				}
				else
				{
					return -1;
				}
			}

			if (right < 4 || left < 4)
			{
				if (right > left)
				{
					return 1;
				}
				else
				{
					return -1;
				}
			}

			return 0;
		}



		private Node StopMoving()
		{
			return new Action(() => Move(0));
		}

		private Node RandomFire() {
            return new Action(() => Fire(UnityEngine.Random.Range(0.0f, 1.0f)));
        }

		//*** a better version of professor's RandomFire
		private Node CalculatedFire(float enemyV)
		{
			return new Action(() => Fire(getPower(enemyV)));
		}


		//*** calculate power needed to *correctly* aim using projectile collision/projectile motion formula
		//*** assuming that projectile mass = 1, no friction and angle X = 10 degrees
		//*** and assuming that 'charging time' isnt taking into account
		private float getPower(float enemyV)
		{
			Vector3 targetPos = TargetTransform().position;
			Vector3 localPos = this.transform.InverseTransformPoint(targetPos);
			float S = localPos.magnitude;
			float gravityPull = Physics.gravity.y / 2f;
			//***This assumes enemy is not moving
			if (enemyV == 0)
			{
				float angle = Mathf.Cos(10 * Mathf.Deg2Rad) * Mathf.Sin(10 * Mathf.Deg2Rad);
				float final = Mathf.Sqrt(-S * gravityPull / angle) / (30f * (50f / S));
				return final;
			}
			//***this assume enemy is moving, (since i don't know how to collect enemy speed, i only assume they are at max speed, 12f units 
			else
			{

				float a = Mathf.Cos(10 * Mathf.Deg2Rad) * Mathf.Sin(10 * Mathf.Deg2Rad);
				float b = enemyV * Mathf.Cos(10 * Mathf.Deg2Rad) * Mathf.Sin(10 * Mathf.Deg2Rad);
				float c = S * gravityPull;

				if (b * b < c * a * 4)
				{
					return getPower(0);
				}
				else
				{
					float cal = Mathf.Sqrt((b * b) - c * a * 4);

					float val1 = (-b + cal)/ (2 * a);
					float val2 = (-b - cal) / (2 * a);
					if (val1 > val2)
					{

						return val1 / (30f * (50f / S));
					}
					else
					{
						return val2 / (30f * (50f / S));
					}
				}
			}
		}


		//***dodging
		private Node Dodge(float dodgeRange,float speed, bool dodge,bool defensiveDodge, bool counter) 
		{
			return new BlackboardCondition("forward", Operator.IS_GREATER_OR_EQUAL, 6f, Stops.IMMEDIATE_RESTART,

				new BlackboardCondition("dodge", Operator.IS_EQUAL, dodge, Stops.IMMEDIATE_RESTART,
					new BlackboardCondition("seeShell", Operator.IS_SMALLER_OR_EQUAL, dodgeRange, Stops.IMMEDIATE_RESTART,
						//***gives up on dodging shells if its too close to AI 
						new Selector(
							//*** countering
							//***check if AI can counter Fire enemy shells
							new BlackboardCondition("counter", Operator.IS_EQUAL, counter, Stops.IMMEDIATE_RESTART,
								new BlackboardCondition("inSight", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
									new BlackboardCondition("targetAhead", Operator.IS_SMALLER_OR_EQUAL, 1f, Stops.IMMEDIATE_RESTART,
										new BlackboardCondition("targetAhead", Operator.IS_GREATER_OR_EQUAL, 0.75f, Stops.IMMEDIATE_RESTART,
											new Sequence(
												CalculatedFire(0f),
												getDodgeTurn(defensiveDodge),
												//getNextTurn(),
												new Selector(
													new BlackboardCondition("targetDistance", Operator.IS_SMALLER_OR_EQUAL, 6f, Stops.IMMEDIATE_RESTART,
														new Action(() => Move(-speed))
													),
													//*** AI something kills itself by walking into it's own shells
													new BlackboardCondition("seeShell", Operator.IS_SMALLER_OR_EQUAL, 12f, Stops.IMMEDIATE_RESTART,
														new Action(() => Move(speed * 0.5f))
													),
													new BlackboardCondition("defensiveDodge", Operator.IS_EQUAL, defensiveDodge, Stops.IMMEDIATE_RESTART,
														new Action(() => Move(ObjectDistance(-this.transform.forward) < 4f ? 0 : -speed))
													),
													new Action(() => Move(ObjectDistance(this.transform.forward) < 4f ? 0 : speed))
												)
											)
										)
									)
								)
							),
							new Sequence(
								getDodgeTurn(defensiveDodge),
								//getNextTurn(),
								new Selector(
									new BlackboardCondition("defensiveDodge", Operator.IS_EQUAL, defensiveDodge, Stops.IMMEDIATE_RESTART,
										new Action(() => Move(ObjectDistance(-this.transform.forward) < 4f ? 0 : -speed))
									),
									new Action(() => Move(ObjectDistance(this.transform.forward) < 4f ? 0 : speed))
								)
							)
						)
					)
				)
			);
		}




		//***pathfinding
		private Node PathFind(float speed, bool pathFind)
		{
			return new BlackboardCondition("pathFind", Operator.IS_EQUAL, pathFind, Stops.IMMEDIATE_RESTART,
				new BlackboardCondition("inSight", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART,
					new Selector(
						//***if theres nothing in front of the AI it move forward and stop turning
						new BlackboardCondition("forward", Operator.IS_GREATER_OR_EQUAL, 4f, Stops.IMMEDIATE_RESTART,
						new Sequence(
							new Action(() => Move(speed)),
							StopTurning()
						)
						),
						//***if something in front of AI, it turn and stop moving
						new Sequence(
							getNextTurn(),
							StopMoving(),
							new Wait(0.15f)
						)
					)
				)
			);
		}


		//***Navmsh
		private Node NavMesh(float speed,float sightDistance, bool pathFind)
		{
			_navMeshAgent.stoppingDistance = sightDistance;
			return new BlackboardCondition("pathFind", Operator.IS_EQUAL, pathFind, Stops.IMMEDIATE_RESTART,
				new BlackboardCondition("inSight", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART,
					new Sequence(
						new Action(() => Move(speed)),
						StopTurning(),
						new Action(() => SetDisination())
					)
				)
			);
		}


		//*** turning based on environment
		private Node Turning(float turnRate, float speed)
		{
			//***turns to enemy if not facing directly at it
			return new BlackboardCondition("targetOffCentre", Operator.IS_GREATER_OR_EQUAL, 0.05f, Stops.IMMEDIATE_RESTART,

				//***stop moving if theres a non enemy in front of it , it still turns to enemy 
				new Selector(

					//*** idk if compound conditional statments exist
					new Selector(
						new BlackboardCondition("forward", Operator.IS_SMALLER_OR_EQUAL, 4f, Stops.IMMEDIATE_RESTART,
							new Selector(
								new BlackboardCondition("targetOnRight", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
									new Sequence(
										StopMoving(),
										new Action(() => Turn(turnRate))
									)
								),
								new BlackboardCondition("targetOnRight", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART,
									new Sequence(
										StopMoving(),
										new Action(() => Turn(-turnRate))
									)
								)
							)
						),
						new BlackboardCondition("behind", Operator.IS_SMALLER_OR_EQUAL, 4f, Stops.IMMEDIATE_RESTART,
							new Selector(
								new BlackboardCondition("targetOnRight", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
									new Sequence(
										StopMoving(),
										new Action(() => Turn(turnRate))
									)
								),
								new BlackboardCondition("targetOnRight", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART,
									new Sequence(
										StopMoving(),
										new Action(() => Turn(-turnRate))
									)
								)
							)
						),
						new BlackboardCondition("targetDistance", Operator.IS_SMALLER_OR_EQUAL, 4f, Stops.IMMEDIATE_RESTART,
							new Selector(
								new BlackboardCondition("targetOnRight", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
									new Sequence(
										new Action(() => Move(-speed)),
										new Action(() => Turn(-turnRate))
									)
								),
								new BlackboardCondition("targetOnRight", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART,
									new Sequence(
										new Action(() => Move(-speed)),
										new Action(() => Turn(turnRate))
									)
								)
							)
						)
					),
					//***same thing but allows tank to move, idk how to use 'Sequence' cos im too fuckin' lazy to read the documentation
					new Selector(
						new BlackboardCondition("targetOnRight", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
							new Action(() => Turn(turnRate))
						),
						new BlackboardCondition("targetOnRight", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART,
							new Action(() => Turn(-turnRate))
						)
					)
				)


			//*** old
			//new Selector(
			//	new BlackboardCondition("targetOnRight", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
			//		new Action(() => Turn(turnRate))
			//	),
			//	new BlackboardCondition("targetOnRight", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART,
			//		new Action(() => Turn(-turnRate))
			//	)
			//)
			);
		}


		//***move and turns to enemy
		private Node MoveToEnemy(float turnRate, float speed, float sightDistance, bool forwardMove)
		{
			//***move towards enemy depending on sightDistance
			return new BlackboardCondition("targetDistance", Operator.IS_GREATER_OR_EQUAL, sightDistance, Stops.IMMEDIATE_RESTART,
				new Selector(
					//***stop moving forward if theres a building in front or enemy is near AI
					new BlackboardCondition("forwardMove", Operator.IS_EQUAL, forwardMove, Stops.IMMEDIATE_RESTART,
						new BlackboardCondition("targetDistance", Operator.IS_GREATER_OR_EQUAL, 4f, Stops.IMMEDIATE_RESTART,
							new BlackboardCondition("forward", Operator.IS_GREATER_OR_EQUAL, 4f, Stops.IMMEDIATE_RESTART,
								new Action(() => Move(speed))
							)
						)
					),

					new Sequence(
						new Selector(
							new BlackboardCondition("targetOnRight", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
								new Action(() => Turn(turnRate))
							),
							new BlackboardCondition("targetOnRight", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART,
								new Action(() => Turn(-turnRate))
							)
						),
						StopMoving()
					)


				//*** old
				//new BlackboardCondition("targetOnRight", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
				//	new Sequence(
				//		new Action(() => Turn(turnRate)),
				//		StopMoving()
				//	)

				//),
				//new BlackboardCondition("targetOnRight", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART,
				//	new Sequence(
				//		new Action(() => Turn(-turnRate)),
				//		StopMoving()
				//	)
				//)
				)
			);
		}

		private Node MoveBackwards(float turnRate, float speed,float fireRate, float sightDistance, float backwardModifier, bool backwardMove, bool backwardFire)
		{
			//*** move backwards if the enemy is too close
			return new BlackboardCondition("backwardMove", Operator.IS_EQUAL, backwardMove, Stops.IMMEDIATE_RESTART,
				new BlackboardCondition("targetDistance", Operator.IS_SMALLER_OR_EQUAL, sightDistance * backwardModifier, Stops.IMMEDIATE_RESTART,
					new BlackboardCondition("inSight", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
						//***if theres nothing behind AI it will go backwards, and shoot
						new Selector(
							new BlackboardCondition("behind", Operator.IS_GREATER_OR_EQUAL, 4f, Stops.IMMEDIATE_RESTART,
								new Selector(
									new BlackboardCondition("backwardFire", Operator.IS_EQUAL, backwardFire, Stops.IMMEDIATE_RESTART,
										new Sequence(
											new Action(() => Move(-speed)),
											CalculatedFire(12f)
										)
									),
									new Sequence(
										new Action(() => Move(-speed))
									)
								)
							),
							//***will trigger if cornered
							new Sequence(
								StopMoving(),
								StopTurning(),
								new Wait(fireRate),
								CalculatedFire(0)
							)
						)
					)
				)
			);
		}


		//***fires at the target
		private Node FireTarget(float fireRate, bool fire)
		{
			//*** fire to enemy if in sight
			return new BlackboardCondition("fire", Operator.IS_EQUAL, fire, Stops.IMMEDIATE_RESTART,
				new BlackboardCondition("inSight", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
					new Sequence(
						StopTurning(),
						StopMoving(),
						new Wait(fireRate),
						CalculatedFire(0)
					)
				)
			);
		}


		//classMagna
		//***dynamic modifications
		private Root ClassMagna(float turnRate, float speed, float fireRate, float sightDistance, float backwardModifier, float dodgeRange, bool pathFind, bool dodge,bool defensiveDodge, bool counter, bool forwardMove, bool backwardMove, bool backwardFire, bool fire)
		{
			return new Root(
				new Service(0.1f, UpdatePerception,
					new Selector(
						//***dodge incoming bullets
						Dodge( dodgeRange,  speed,  dodge, defensiveDodge, counter),
						//***pathfinding
						new Selector(
							//***check if AI can see enemy,if it cant then crappy pathfind

							//PathFind(speed,pathFind),
							NavMesh(speed, sightDistance, pathFind),

							//***turns to enemy if not facing directly at it
							Turning(turnRate, speed)
						),
						//***action
						new Selector(
							//***move towards enemy depending on sightDistance
							MoveToEnemy(turnRate, speed, sightDistance, forwardMove),
							//*** move backwards if the enemy is too close
							MoveBackwards( turnRate,  speed, fireRate, sightDistance, backwardModifier,  backwardMove,  backwardFire),
							//*** fire to enemy if in sight
							FireTarget(fireRate, fire)
						),
						//*** spaghetti code
						StopMoving(),
						StopTurning()
					)
				)
			);
		}

		// Constantly spin and fire on the spot 
		private Root SpinBehaviour(float turn, float shoot) {
            return new Root(new Sequence(
                        new Action(() => Turn(turn)),
                        new Action(() => Fire(shoot))
                    ));
        }

        // Turn to face your opponent and fire
        private Root TrackBehaviour() {
            return new Root(
                new Service(0.2f, UpdatePerception,
                    new Selector(
                        new BlackboardCondition("targetOffCentre",
                                                Operator.IS_SMALLER_OR_EQUAL, 0.1f,
                                                Stops.IMMEDIATE_RESTART,
                            // Stop turning and fire
                            new Sequence(StopTurning(),
                                        new Wait(2f),
                                        RandomFire())),
                        new BlackboardCondition("targetOnRight",
                                                Operator.IS_EQUAL, true,
                                                Stops.IMMEDIATE_RESTART,
                            // Turn right toward target
                            new Action(() => Turn(0.2f))),
                            // Turn left toward target
                            new Action(() => Turn(-0.2f))
                    )
                )
            );
        }

        private void UpdatePerception() {

			//used for enabling or disabling features
			blackboard["pathFind"] = true;
			blackboard["dodge"] = true;
			blackboard["defensiveDodge"] = true;
			blackboard["counter"] = true;
			blackboard["forwardMove"] = true;
			blackboard["backwardMove"] = true;
			blackboard["backwardFire"] = true;
			blackboard["fire"] = true;

			Vector3 targetPos = TargetTransform().position;
            Vector3 localPos = this.transform.InverseTransformPoint(targetPos);
            Vector3 heading = localPos.normalized;

			blackboard["targetDistance"] = localPos.magnitude;
			//print(localPos.magnitude);

			blackboard["inSight"] = InSight();

			blackboard["behind"] = ObjectDistance(-1 * this.transform.forward);
			blackboard["forward"] = ObjectDistance(1 * this.transform.forward);

			blackboard["seeShell"] = seeShell();

			blackboard["targetInFront"] = heading.z > 0;
            blackboard["targetOnRight"] = heading.x > 0;


			Vector3 getDirection = (targetPos - this.transform.position).normalized;
			blackboard["targetAhead"] = Vector3.Dot(this.transform.forward, getDirection);

			blackboard["targetOffCentre"] = Mathf.Abs(heading.x);
        }

    }
}