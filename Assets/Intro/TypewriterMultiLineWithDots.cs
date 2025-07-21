using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TypewriterMultiLineWithDots : MonoBehaviour
{
    public TextMeshProUGUI targetText;
    [TextArea] public string fullText;
    public float typingDelay = 0.04f;
    public AudioSource typingSound;

    [Tooltip("DOT 애니메이션을 적용할 줄 번호 (0부터 시작)")]
    public List<int> dotLineIndexes = new List<int>();

    public float dotInterval = 0.5f;
    public int maxDots = 3;

    private Coroutine typingCoroutine;
    private List<Coroutine> dotCoroutines = new();

    private string[] lines;
    private string[] finalLines;
    private string[] baseLines; // > 없이 원본 저장용

    public void StartTyping()
    {
        StopAllCoroutines();
        foreach (var coroutine in dotCoroutines)
        {
            StopCoroutine(coroutine);
        }
        dotCoroutines.Clear();
        targetText.text = "";

        lines = fullText.Split('\n');
        finalLines = new string[lines.Length];
        baseLines = new string[lines.Length];

        typingCoroutine = StartCoroutine(TypeLines());
    }

    private IEnumerator TypeLines()
    {
        for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            string line = lines[lineIndex];

            if (string.IsNullOrWhiteSpace(line))
            {
                finalLines[lineIndex] = "";
                baseLines[lineIndex] = "";
                UpdateAllLines();
                yield return new WaitForSeconds(typingDelay * 5);
                continue;
            }

            baseLines[lineIndex] = line;

            // >만 앞에 붙임
            for (int i = 0; i <= line.Length; i++)
            {
                string typed = line.Substring(0, i);
                finalLines[lineIndex] = "> " + typed;
                UpdateAllLines();

                if (i < line.Length && typingSound && !char.IsWhiteSpace(line[i]))
                    typingSound.Play();

                yield return new WaitForSeconds(typingDelay);
            }

            // 텍스트가 완전히 타이핑된 후에 > 추가
            finalLines[lineIndex] = "> " + baseLines[lineIndex];
            UpdateAllLines();

            // 점 애니메이션을 시작하기 전에 > 텍스트가 완성된 상태로 업데이트
            if (dotLineIndexes.Contains(lineIndex))
            {
                // 점 애니메이션 시작 전에 타이핑이 완료된 상태로 확정
                Coroutine dotCoroutine = StartCoroutine(AnimateDots(lineIndex));
                dotCoroutines.Add(dotCoroutine);
            }

            yield return new WaitForSeconds(typingDelay * 3);
        }
    }

    // 점 애니메이션 추가
    private IEnumerator AnimateDots(int lineIndex)
    {
        int dotCount = 0;
        // 점 애니메이션은 이미 >가 추가된 후 시작됨
        while (true)
        {
            // 점 애니메이션을 적용할 줄 번호만 처리
            string dots = new string('.', dotCount);
            finalLines[lineIndex] = $"> {baseLines[lineIndex]}{dots}";
            UpdateAllLines();
            dotCount = (dotCount + 1) % (maxDots + 1);
            yield return new WaitForSeconds(dotInterval);
        }
    }

    private void UpdateAllLines()
    {
        string result = "";
        for (int i = 0; i < finalLines.Length; i++)
        {
            result += (finalLines[i] ?? "") + "\n";
        }
        targetText.text = result.TrimEnd(); // 마지막 개행 제거
    }
}
